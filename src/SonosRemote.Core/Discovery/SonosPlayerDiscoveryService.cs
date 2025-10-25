using OpenSource.UPnP;
using SonosRemote.Core.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace SonosRemote.Core.Discovery {

	// SEE: https://github.com/ByteDev/ByteDev.Sonos/blob/master/src/ByteDev.Sonos.Upnp.Discovery/SonosDiscoveryService.cs
	public class SonosPlayerDiscoveryService {

		public const string DeviceType = "urn:schemas-upnp-org:device:ZonePlayer:1";

		public EventHandler<SonosPlayerEventArgs> PlayerAdded;
		public EventHandler<SonosPlayerEventArgs> PlayerChanged;
		public EventHandler<SonosPlayerEventArgs> PlayerRemoved;
		
		public ReadOnlyCollection<SonosPlayer> Players {
			get => new ReadOnlyCollection<SonosPlayer>([.. PlayersById.Values]);
		}

		protected readonly ConcurrentDictionary<string, SonosPlayer> PlayersById;
		protected UPnPSmartControlPoint ControlPoint { get; private set; }

		public SonosPlayerDiscoveryService() {

			PlayersById = new ConcurrentDictionary<string, SonosPlayer>(StringComparer.InvariantCulture);
		}

		public virtual void Scan() {

			if (ControlPoint == null) {
				InitializeControlPoint();
			} else {
				ControlPoint.Rescan();
			}
		}

		public virtual void Rescan() {

			PlayersById.Clear();
			DisposeControlPoint();
			InitializeControlPoint();
		}

		public virtual void Stop() {

			DisposeControlPoint();
		}

		private void OnDeviceAdded(UPnPSmartControlPoint sender, UPnPDevice upnpDevice) {

			var player = SonosPlayer.CreateFromUpnpDevice(upnpDevice);
			var id = player.ID;

			if (PlayersById.TryAdd(id, player)) {
				OnSonosPlayerAdded(player);
			} else if (PlayersById.TryGetValue(id, out var existing)) {
				if (player != existing) {
					if (PlayersById.TryUpdate(id, player, existing)) {
						OnSonosPlayerChanged(player);
					}
				}
			}			
		}

		private void OnDeviceRemoved(UPnPSmartControlPoint sender, UPnPDevice upnpDevice) {

			var player = SonosPlayer.CreateFromUpnpDevice(upnpDevice);
			var id = player.ID;

			if (PlayersById.TryRemove(id, out var removed)) {
				OnSonosPlayerRemoved(removed);
			}
		}

		private void OnServiceAdded(UPnPSmartControlPoint sender, UPnPService service) {

			// Do nothing
		}

		private void OnServiceRemoved(UPnPSmartControlPoint sender, UPnPService service) {

			// Do nothing
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

		private void InitializeControlPoint() {
			
			if (ControlPoint != null) {
				return;
			}

			/// NOTE: The UPnPSmartControlPoint will automatically start searching for devices upon creation, and
			/// internally, it uses a static class which retains state across multiple instances. Therefore,
			/// once a device is found, it will be found by any subsequently created instances of UPnPSmartControlPoint.
			/// The OnDeviceAdded event will be raised for each device found when subsequent instances are created.
			ControlPoint = new UPnPSmartControlPoint(OnDeviceAdded, OnServiceAdded, DeviceType);
			ControlPoint.OnRemovedDevice += OnDeviceRemoved;
			ControlPoint.OnRemovedService += OnServiceRemoved;
		}

		private void DisposeControlPoint() {

			if (ControlPoint == null) {
				return;
			}

			ControlPoint.OnAddedDevice -= OnDeviceAdded;
			ControlPoint.OnRemovedDevice -= OnDeviceRemoved;
			ControlPoint.OnAddedService -= OnServiceAdded;
			ControlPoint.OnRemovedService -= OnServiceRemoved;

			ControlPoint = null;
		}
	}
}