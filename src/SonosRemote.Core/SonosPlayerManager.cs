using SonosRemote.Core.Discovery;
using SonosRemote.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SonosRemote.Core {

	/// <summary>
	/// Scans for Sonos players on the network and monitors their states.
	/// </summary>
	/// <remarks>
	/// This class runs two threads in the background:
	///		1) A thread that periodically scans for Sonos players on the network.
	///		2) A thread that periodically scans for the states of the discovered players. 
	/// </remarks>
	public class SonosPlayerManager : IDisposable {

		public const int ScanForPlayersDefaultTimeoutMS = 20 * 1000; // 20 seconds
		public const int ScanForPlayersDefaultIntervalMS = 10 * 60 * 1000; // 10 minutes
		public const int ScanForPlayerStatesDefaultIntervalMS = 2 * 1000; // 2 seconds

		public EventHandler<SonosPlayerEventArgs> PlayerAdded;
		public EventHandler<SonosPlayerEventArgs> PlayerChanged;
		public EventHandler<SonosPlayerEventArgs> PlayerRemoved;

		public EventHandler<SonosPlayerStateEventArgs> PlayerStateAdded;
		public EventHandler<SonosPlayerStateEventArgs> PlayerStateChanged;
		public EventHandler<SonosPlayerStateEventArgs> PlayerStateRemoved;

		internal EventHandler ScanForPlayerStatesComplete;

		public ReadOnlyCollection<SonosPlayer> Players {
			get => new ReadOnlyCollection<SonosPlayer>([.. PlayersByName.Values]);
		}

		protected static readonly object mScanForPlayersLock = new();

		protected int ScanForPlayersTimeoutMS { get; set; }
		protected int ScanForPlayersIntervalMS { get; set; }
		protected int ScanForPlayerStatesIntervalMS { get; set; }

		protected bool IsScanningForPlayers { get; set; }
		protected bool IsScanningForPlayerStates { get; set; }

		protected SonosPlayerDiscoveryService DiscoveryService { get; set; }
		protected CancellationTokenSource CancellationTokenSource { get; private set; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		protected readonly ConcurrentDictionary<string, SonosPlayer> CurrentScan;
		protected readonly ConcurrentDictionary<string, SonosPlayer> PlayersByName;
		protected readonly ConcurrentDictionary<string, SonosPlayerState> PlayerStatesByName;

		private bool mDisposedValue;

		public SonosPlayerManager() {

			ScanForPlayersTimeoutMS = ScanForPlayersDefaultTimeoutMS;
			ScanForPlayersIntervalMS = ScanForPlayersDefaultIntervalMS;
			ScanForPlayerStatesIntervalMS = ScanForPlayerStatesDefaultIntervalMS;

			IsScanningForPlayers = false;
			IsScanningForPlayerStates = false;

			DiscoveryService = new SonosPlayerDiscoveryService();
			CancellationTokenSource = new CancellationTokenSource();

			CurrentScan = new ConcurrentDictionary<string, SonosPlayer>(StringComparer.InvariantCulture);
			PlayersByName = new ConcurrentDictionary<string, SonosPlayer>(StringComparer.InvariantCulture);
			PlayerStatesByName = new ConcurrentDictionary<string, SonosPlayerState>(StringComparer.InvariantCulture);

			DiscoveryService.PlayerAdded += HandlePlayerFound;
			DiscoveryService.PlayerChanged += HandlePlayerChanged;
			DiscoveryService.PlayerRemoved += HandlePlayerRemoved;
		}

		public virtual void Start() {

			ScanForPlayers();
			StartMonitorThreads();
		}

		public virtual void Stop() {

			CancellationTokenSource.Cancel();
			DiscoveryService.Stop();
		}

		public virtual SonosPlayerState GetPlayerState(SonosPlayer player) {

			if (PlayerStatesByName.TryGetValue(player.UniqueDeviceName, out var state)) {
				return state;
			}

			return null;
		}

		public virtual bool TryGetPlayerState(SonosPlayer player, out SonosPlayerState value) {

			return PlayerStatesByName.TryGetValue(player.ID, out value);
		}

		protected virtual Task ScanForPlayers() {

			var result = Task.CompletedTask;

			if (!IsScanningForPlayers) {
				lock (mScanForPlayersLock) {
					if (!IsScanningForPlayers) {
						IsScanningForPlayers = true;
						
						try {
							OnScanForPlayersStarted();
							DiscoveryService.Rescan();
							result = Task
								.Delay(ScanForPlayersTimeoutMS, CancellationToken)
								.ContinueWith((task) => {
									IsScanningForPlayers = false;
									OnScanForPlayersCompleted();
								});
						} catch {
							IsScanningForPlayers = false;
						}
					}
				}
			}

			return result;
		}

		protected virtual void StartMonitorThreads() {

			if (CancellationTokenSource.IsCancellationRequested) {
				CancellationTokenSource = new CancellationTokenSource();
			}

			var scanForPlayers = new Thread(ScanForPlayersThread) {
				IsBackground = true
			};

			var scanForPlayerStates = new Thread(ScanForPlayerStatesThread) {
				IsBackground = true
			};

			scanForPlayers.Start();
			scanForPlayerStates.Start();
		}

		protected virtual void OnScanForPlayersStarted() {

			CurrentScan.Clear();
		}

		protected virtual void OnScanForPlayersCompleted() {

			var removed = PlayersByName.Keys.Except(CurrentScan.Keys).ToList();
			
			foreach (var id in removed) {
				if (PlayersByName.TryRemove(id, out var player)) {
					OnSonosPlayerRemoved(player);
				}
			}
		}

		protected virtual void HandlePlayerFound(object sender, SonosPlayerEventArgs e) {

			// TODO: Should check for IsScanningForPlayers here? Not sure how
			// likely it is that a player is found long after the scan is requested,
			// will need to test this.

			var id = e.ID;
			var player = e.Player;

			//Console.WriteLine($"Found player: {player.ID}");

			if (PlayersByName.TryAdd(id, player)) {
				OnSonosPlayerAdded(player);
			}

			CurrentScan.TryAdd(id, player);
		}

		protected virtual void HandlePlayerChanged(object sender, SonosPlayerEventArgs e) {

			// TODO: Should check for IsScanningForPlayers here? Not sure how
			// likely it is that a player is found long after the scan is requested,
			// will need to test this.

			var id = e.ID;
			var player = e.Player;

			if (PlayersByName.TryGetValue(id, out var existing)) {
				if (PlayersByName.TryUpdate(id, player, existing)) {
					OnSonosPlayerChanged(player);
				}
			}
		}

		protected virtual void HandlePlayerRemoved(object sender, SonosPlayerEventArgs e) {
			
			var id = e.ID;
			var player = e.Player;

			if (PlayersByName.TryRemove(id, out var removed)) {
				OnSonosPlayerRemoved(removed);
			}
		}

		protected virtual void ScanForPlayersThread(object obj) {

			while (!CancellationToken.IsCancellationRequested) {
				Task.Delay(ScanForPlayersIntervalMS, CancellationToken).Wait();

				ScanForPlayers();
			}
		}

		protected virtual void ScanForPlayerStatesThread(object obj) {

			while (!CancellationToken.IsCancellationRequested) {
				Task.Delay(ScanForPlayerStatesIntervalMS, CancellationToken).Wait();

				var now = DateTimeOffset.UtcNow;

				foreach (var player in PlayersByName.Values.ToArray()) {

					if (PlayerStatesByName.TryGetValue(player.UniqueDeviceName, out var state)) {
						var updated = state.UpdateAsync().GetAwaiter().GetResult();

						if (state != updated) {
							if (PlayerStatesByName.TryUpdate(player.UniqueDeviceName, updated, state)) {
								OnSonosPlayerStateChanged(updated);
							}
						}
					} else {
						state = SonosPlayerState.CreateAsync(player).GetAwaiter().GetResult();

						//Console.WriteLine($"Created player state for: {state.Device.RoomName}");

						if (PlayerStatesByName.TryAdd(player.UniqueDeviceName, state)) {
							OnSonosPlayerStateAdded(state);
						}
					}
				}

				//Console.WriteLine($"Thread took {(DateTimeOffset.UtcNow - now).TotalMilliseconds}ms");

				OnScanForPlayerStatesComplete();
			}
		}

		protected virtual void OnSonosPlayerAdded(SonosPlayer value) {
			PlayerAdded?.Invoke(this, new SonosPlayerEventArgs(value, EventAction.Added));
		}

		protected virtual void OnSonosPlayerChanged(SonosPlayer value) {
			PlayerChanged?.Invoke(this, new SonosPlayerEventArgs(value, EventAction.Changed));
		}

		protected virtual void OnSonosPlayerRemoved(SonosPlayer value) {
			PlayerRemoved?.Invoke(this, new SonosPlayerEventArgs(value, EventAction.Removed));
		}

		protected virtual void OnSonosPlayerStateAdded(SonosPlayerState value) {
			PlayerStateAdded?.Invoke(this, new SonosPlayerStateEventArgs(value, EventAction.Added));
		}

		protected virtual void OnSonosPlayerStateChanged(SonosPlayerState value) {
			PlayerStateChanged?.Invoke(this, new SonosPlayerStateEventArgs(value, EventAction.Changed));
		}

		protected virtual void OnSonosPlayerStateRemoved(SonosPlayerState value) {
			PlayerStateRemoved?.Invoke(this, new SonosPlayerStateEventArgs(value, EventAction.Removed));
		}

		protected virtual void OnScanForPlayerStatesComplete() {
			ScanForPlayerStatesComplete?.Invoke(this, EventArgs.Empty);
		}

		protected virtual void Dispose(bool disposing) {
			if (!mDisposedValue) {
				if (disposing) {
					CancellationTokenSource.Cancel();
					CancellationTokenSource.Dispose();
					DiscoveryService.Stop();
					DiscoveryService.PlayerAdded -= HandlePlayerFound;
					DiscoveryService.PlayerChanged -= HandlePlayerChanged;
					DiscoveryService.PlayerRemoved -= HandlePlayerRemoved;
				}

				DiscoveryService = null;
				CancellationTokenSource = null;

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
