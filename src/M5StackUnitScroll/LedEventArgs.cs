using System;

namespace M5StackUnitScroll {
	public class LedEventArgs : EventArgs {

		public byte R { get; }

		public byte G { get; }

		public byte B { get; }

		public LedEventArgs(byte r, byte g, byte b) {
			R = r;
			G = g;
			B = b;
		}
	}
}
