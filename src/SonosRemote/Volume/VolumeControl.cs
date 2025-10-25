using M5StackUnitScroll;
using System;

namespace SonosRemote.Volume {

	public class VolumeControl : IDisposable {

		public event EventHandler<VolumeEventArgs> VolumeChanged;

		public int Volume { get; set; }

		public int MaxVolume { get; set; }

		protected UnitScroll UnitScroll { get; private set; }

		private bool mDisposedValue;

		public VolumeControl(UnitScroll unitScroll, int maxVolume = 100) {

			UnitScroll = unitScroll;
			MaxVolume = maxVolume;

			UnitScroll.EncoderChanged += HandleEncoderChanged;
		}

		private void HandleEncoderChanged(object sender, EncoderEventArgs e) {

			var delta = UnitScroll.GetIncEncoderValue();
			var value = Math.Min(Math.Max(0, Volume + delta), MaxVolume);

			if (value != Volume) {
				Volume = value;
				OnVolumeChanged(new VolumeEventArgs(Volume));
			}
		}

		protected virtual void OnVolumeChanged(VolumeEventArgs e) {
			VolumeChanged?.Invoke(this, e);
		}

		protected virtual void Dispose(bool disposing) {

			if (!mDisposedValue) {
				if (disposing) {
					UnitScroll.EncoderChanged -= HandleEncoderChanged;
					UnitScroll = null;
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
