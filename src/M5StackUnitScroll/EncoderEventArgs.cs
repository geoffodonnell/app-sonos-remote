using System;

namespace M5StackUnitScroll {
	public class EncoderEventArgs : EventArgs { 
	
		public int Value { get; set; }

		public EncoderEventArgs(int value) { 
			Value = value;
		}
	}
}
