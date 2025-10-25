using System;

namespace M5StackUnitScroll {
	public class ButtonEventArgs : EventArgs {

		public bool IsPressed { get; }

		public ButtonEventArgs(bool isPressed) {
			IsPressed = isPressed;
		}
	}
}
