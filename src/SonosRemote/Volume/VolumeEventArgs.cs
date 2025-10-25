using System;

namespace SonosRemote.Volume {

	public class VolumeEventArgs : EventArgs {

		public int Volume { get; }

		public VolumeEventArgs(int volume) {
			Volume = volume;
		}
	}
}
