using ByteDev.Sonos;
using ByteDev.Sonos.Device;
using ByteDev.Sonos.Models;
using ByteDev.Sonos.Upnp.Services;
using SonosRemote.Core.Model;
using System;
using System.Threading.Tasks;

namespace SonosRemote.Core {

	public class SonosPlayerState : IEquatable<SonosPlayerState> {

		protected static readonly SonosControllerFactory ControllerFactory = new();

		protected static readonly SonosDeviceService DeviceService = new();

		protected SonosController Controller { get; set; }

		protected IAvTransportService AvTransportService { get; set; }

		protected IContentDirectoryService ContentDirectoryService { get; set; }

		protected IRenderingControlService RenderingControlService { get; set; }

		public string ID => Player?.ID;

		public DateTimeOffset Created { get; set; }

		public DateTimeOffset Updated { get; set; }

		public DateTimeOffset Changed { get; set; }

		public SonosPlayer Player { get; protected set; }

		public string IpAddress => Device.IpAddress;

		public string UniqueDeviceName => Player.UniqueDeviceName;

		public SonosDevice Device { get; protected set; }

		public bool IsPlaying { get; protected set; }

		public int Volume { get; protected set; }

		public int MinVolume { get; protected set; }

		public int MaxVolume { get; protected set; }

		public SonosPositionInfo CurrentTrack { get; protected set; }

		protected SonosPlayerState() {

			// Hide default constructor
		}

		public async Task<SonosPlayerState> UpdateAsync() {

			var volume = Controller.GetVolumeAsync();
			var playing = Controller.GetIsPlayingAsync();
			var track = AvTransportService.GetPositionInfoAsync();

			await Task.WhenAll(volume, playing, track);

			var currentTrackValue = SonosPositionInfo.Create(track.Result);
			var isPlayingValue = playing.Result;
			var volumeValue = volume.Result.Value;

			return new SonosPlayerState {
				Player = Player,
				Controller = Controller,
				Device = Device,
				AvTransportService = AvTransportService,
				ContentDirectoryService = ContentDirectoryService,
				RenderingControlService = RenderingControlService,
				IsPlaying = isPlayingValue,
				Volume = volumeValue,
				MinVolume = SonosVolume.MinVolume,
				MaxVolume = SonosVolume.MaxVolume,
				CurrentTrack = currentTrackValue
			};
		}

		public async Task SetVolumeAsync(int volume) {
			
			await Controller.SetVolumeAsync(new SonosVolume(volume));
		}

		public async Task SetPlayingAsync(bool playing) {

			if (playing) {
				await Controller.PlayAsync();
			} else {
				await Controller.PauseAsync();
			}
		}

		public override bool Equals(object obj) {

			return Equals(obj as SonosPlayerState);
		}

		public override int GetHashCode() {

			return HashCode.Combine(
				IpAddress,
				UniqueDeviceName,
				IsPlaying,
				Volume,
				MinVolume,
				MaxVolume,
				CurrentTrack);
		}

		public bool Equals(SonosPlayerState other) {

			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(SonosPlayerState obj1, SonosPlayerState obj2) {

			if (ReferenceEquals(obj1, obj2)) {
				return true;
			}

			if (ReferenceEquals(obj1, null)) {
				return false;
			}

			if (ReferenceEquals(obj2, null)) {
				return false;
			}

			return obj1.Equals(obj2);
		}

		public static bool operator !=(SonosPlayerState obj1, SonosPlayerState obj2) {

			return !(obj1 == obj2);
		}

		public static async Task<SonosPlayerState> CreateAsync(SonosPlayer player) {

			var ip = player.RemoteEndPoint.Address.ToString();
			var controller = ControllerFactory.Create(ip);

			var avTransport = new AvTransportService(ip);
			var contentDirectory = new ContentDirectoryService(ip);
			var renderingControl = new RenderingControlService(ip);

			var device = DeviceService.GetDeviceAsync(ip);
			var playing = controller.GetIsPlayingAsync();
			var volume = controller.GetVolumeAsync();
			var currentTrack = avTransport.GetPositionInfoAsync();

			await Task.WhenAll(device, playing, volume, currentTrack);

			return new SonosPlayerState {
				Player = player,
				Controller = controller,
				Device = device.Result,
				AvTransportService = avTransport,
				ContentDirectoryService = contentDirectory,
				RenderingControlService = renderingControl,
				IsPlaying = playing.Result,
				Volume = volume.Result.Value,
				MinVolume = SonosVolume.MinVolume,
				MaxVolume = SonosVolume.MaxVolume,
				CurrentTrack = SonosPositionInfo.Create(currentTrack.Result)
			};
		}
	}
}
