using SonosRemote.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SonosRemote.Core {

	/// <summary>
	/// Monitor players for their groupings and states.
	/// </summary>
	public class SonosPlayerGroupManager : IDisposable {

		public EventHandler<SonosPlayerGroupEventArgs> PlayerGroupAdded;
		public EventHandler<SonosPlayerGroupEventArgs> PlayerGroupChanged;
		public EventHandler<SonosPlayerGroupEventArgs> PlayerGroupRemoved;

		public EventHandler<SonosPlayerGroupStateEventArgs> PlayerGroupStateAdded;
		public EventHandler<SonosPlayerGroupStateEventArgs> PlayerGroupStateChanged;
		public EventHandler<SonosPlayerGroupStateEventArgs> PlayerGroupStateRemoved;

		public ReadOnlyCollection<SonosPlayerGroup> Groups {
			get => new ReadOnlyCollection<SonosPlayerGroup>([.. PlayerGroupsByName.Values]);
		}

		protected static Regex SonosGroupRegex = new(@"^(x-rincon:)([A-Za-z0-9_]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		protected SonosPlayerManager SonosPlayerManager { get; private set; }
		protected ConcurrentDictionary<string, SonosPlayerGroup> PlayerGroupsByName { get; private set; }
		protected ConcurrentDictionary<string, SonosPlayerGroupState> PlayerGroupStatesByName { get; private set; }

		private bool mDisposedValue;

		public SonosPlayerGroupManager(SonosPlayerManager sonosPlayerManager) {

			PlayerGroupsByName = new ConcurrentDictionary<string, SonosPlayerGroup>(StringComparer.InvariantCulture);
			PlayerGroupStatesByName = new ConcurrentDictionary<string, SonosPlayerGroupState>(StringComparer.InvariantCulture);

			SonosPlayerManager = sonosPlayerManager ?? throw new ArgumentNullException(nameof(sonosPlayerManager));
			
			// Groups and group states are both derived from the set of player states,
			// so we don't need to monitor player events, just player state events
			SonosPlayerManager.PlayerStateAdded += HandlePlayerStateAdded;
			SonosPlayerManager.PlayerStateChanged += HandlePlayerStateChanged;
			SonosPlayerManager.PlayerStateRemoved += HandlePlayerStateRemoved;
			SonosPlayerManager.ScanForPlayerStatesComplete += HandleScanForPlayerStatesComplete;
		}

		public virtual bool TryGetPlayerGroupState(SonosPlayerGroup group, out SonosPlayerGroupState value) {

			return PlayerGroupStatesByName.TryGetValue(group.ID, out value);
		}

		public virtual async Task TogglePlayingAsync(SonosPlayerGroup group) {

			if (!TryGetPlayerGroupState(group, out var state)) {
				throw new InvalidOperationException("State not found.");
			}

			var playing = state.MasterState.IsPlaying;

			var tasks = new List<Task>() {
				state.MasterState.SetPlayingAsync(!playing)
			};

			foreach (var member in state.MemberStates) {
				tasks.Add(member.SetPlayingAsync(!playing));
			}

			await Task.WhenAll(tasks);
		}

		public virtual async Task SetVolumeAsync(SonosPlayerGroup group, int volume) {

			if (!TryGetPlayerGroupState(group, out var state)) {
				throw new InvalidOperationException("State not found.");
			}

			var tasks = new List<Task>() {
				state.MasterState.SetVolumeAsync(volume)
			};

			foreach (var member in state.MemberStates) {
				tasks.Add(member.SetVolumeAsync(volume));
			}

			await Task.WhenAll(tasks);
		}

		protected virtual void HandlePlayerStateAdded(object sender, SonosPlayerStateEventArgs e) {

			// Do nothing
		}

		protected virtual void HandlePlayerStateChanged(object sender, SonosPlayerStateEventArgs e) {

			// Do nothing
		}

		protected virtual void HandlePlayerStateRemoved(object sender, SonosPlayerStateEventArgs e) {

			// Do nothing
		}

		protected virtual void HandleScanForPlayerStatesComplete(object sender, EventArgs e) {

			ProcessChanges();
		}

		protected virtual void ProcessChanges() {

			var players = SonosPlayerManager.Players;
			var playerStates = new List<SonosPlayerState>();

			foreach (var player in players) {
				if (SonosPlayerManager.TryGetPlayerState(player, out var playerState)) {
					playerStates.Add(playerState);
				}
			}

			var groupStates = GroupPlayersByState(playerStates);

			ProcessGroupsFromGroupStates(groupStates);
			ProcessGroupStatesFromGroupStates(groupStates);
		}

		protected virtual void ProcessGroupsFromGroupStates(IEnumerable<SonosPlayerGroupState> states) {

			var existingIds = PlayerGroupStatesByName.Keys.ToHashSet(StringComparer.InvariantCultureIgnoreCase);

			// Process additions and modifications
			foreach (var group in states.Select(s => s.Group)) {
				var id = group.ID;

				if (PlayerGroupsByName.TryAdd(id, group)) {
					OnSonosPlayerGroupAdded(group);
				} else if (PlayerGroupsByName.TryGetValue(id, out var existing)) {
					if (group != existing) {
						if (PlayerGroupsByName.TryUpdate(id, group, existing)) { 
							OnSonosPlayerGroupChanged(group);
						}
					}
				}
			}

			// Process removals
			var removedIds = existingIds.Except(PlayerGroupsByName.Keys).ToList();

			foreach (var id in removedIds) {
				if (PlayerGroupsByName.TryRemove(id, out var removed)) {
					OnSonosPlayerGroupRemoved(removed);
				}
			}
		}

		protected virtual void ProcessGroupStatesFromGroupStates(IEnumerable<SonosPlayerGroupState> states) {
			
			var existingIds = PlayerGroupStatesByName.Keys.ToHashSet(StringComparer.InvariantCultureIgnoreCase);
			
			// Process additions and modifications
			foreach (var groupState in states) {
				var id = groupState.ID;

				if (PlayerGroupStatesByName.TryAdd(id, groupState)) {
					OnSonosPlayerGroupStateAdded(groupState);
				} else if (PlayerGroupStatesByName.TryGetValue(id, out var existing)) {
					if (groupState != existing) {
						if (PlayerGroupStatesByName.TryUpdate(id, groupState, existing)) {
							OnSonosPlayerGroupStateChanged(groupState);
						}
					}
				}
			}

			// Process removals
			var removedIds = existingIds.Except(PlayerGroupStatesByName.Keys).ToList();

			foreach (var id in removedIds) {
				if (PlayerGroupStatesByName.TryRemove(id, out var removed)) {
					OnSonosPlayerGroupStateRemoved(removed);
				}
			}
		}

		protected static SonosPlayerGroupState[] GroupPlayersByState(IEnumerable<SonosPlayerState> playerStates) {

			var now = DateTimeOffset.Now;
			var result = new Dictionary<string, SonosPlayerGroupState>();

			foreach (var state in playerStates) {
				var trackUri = state.CurrentTrack.TrackUri;
				var match = SonosGroupRegex.Match(trackUri);

				if (match.Success) {
					var masterUdn = match.Groups[2].Value;

					if (result.TryGetValue(masterUdn, out var groupState)) {
						groupState.Group.Members.Add(state.Player);
						groupState.MemberStates.Add(state);
					} else {
						var master = playerStates
							.FirstOrDefault(s => s.UniqueDeviceName.Equals(masterUdn, StringComparison.InvariantCultureIgnoreCase));

						if (master != null) {
							result.Add(masterUdn, new SonosPlayerGroupState {
								Created = now,
								Updated = now,
								Changed = now,
								Group = new SonosPlayerGroup {
									Master = master.Player,
									Members = new List<SonosPlayer> { master.Player }
								},
								MasterState = master,
								MemberStates = new List<SonosPlayerState> { state }
							});
						}
					}
				}
			}

			return result.Values.ToArray();
		}

		protected virtual void OnSonosPlayerGroupAdded(SonosPlayerGroup value) {
			PlayerGroupAdded?.Invoke(this, new SonosPlayerGroupEventArgs(value, EventAction.Added));
		}

		protected virtual void OnSonosPlayerGroupChanged(SonosPlayerGroup value) {
			PlayerGroupChanged?.Invoke(this, new SonosPlayerGroupEventArgs(value, EventAction.Changed));
		}

		protected virtual void OnSonosPlayerGroupRemoved(SonosPlayerGroup value) {
			PlayerGroupRemoved?.Invoke(this, new SonosPlayerGroupEventArgs(value, EventAction.Removed));
		}

		protected virtual void OnSonosPlayerGroupStateAdded(SonosPlayerGroupState value) {
			PlayerGroupStateAdded?.Invoke(this, new SonosPlayerGroupStateEventArgs(value, EventAction.Added));
		}

		protected virtual void OnSonosPlayerGroupStateChanged(SonosPlayerGroupState value) {
			PlayerGroupStateChanged?.Invoke(this, new SonosPlayerGroupStateEventArgs(value, EventAction.Changed));
		}

		protected virtual void OnSonosPlayerGroupStateRemoved(SonosPlayerGroupState value) {
			PlayerGroupStateRemoved?.Invoke(this, new SonosPlayerGroupStateEventArgs(value, EventAction.Removed));
		}

		protected virtual void Dispose(bool disposing) {
			if (!mDisposedValue) {
				if (disposing) {
					SonosPlayerManager.PlayerStateAdded -= HandlePlayerStateAdded;
					SonosPlayerManager.PlayerStateChanged -= HandlePlayerStateChanged;
					SonosPlayerManager.PlayerStateRemoved -= HandlePlayerStateRemoved;
					SonosPlayerManager.ScanForPlayerStatesComplete -= HandleScanForPlayerStatesComplete;
				}

				mDisposedValue = true;
			}
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
