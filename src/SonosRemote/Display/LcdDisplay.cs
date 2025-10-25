using Iot.Device.CharacterLcd;
using System;
using System.Drawing;

namespace SonosRemote.Display {
	public class LcdDisplay : IDisplay, IDisposable {

		public Size Size => mLcd.Size;

		private Hd44780 mLcd;

		public LcdDisplay(Hd44780 lcd) {
			mLcd = lcd;
		}

		public void Clear() {
			
			mLcd.Clear();
		}

		public void SetCursorPosition(int left, int top) {
			
			mLcd.SetCursorPosition(left, top);
		}

		public void TurnOff() {
			
			mLcd.DisplayOn = false;
			mLcd.BacklightOn = false;
		}

		public void TurnOn() {
			
			mLcd.DisplayOn = true;
			mLcd.BacklightOn = true;
		}

		public void Write(string text) {
			
			mLcd.Write(text);
		}

		public void Dispose() {

			mLcd.Dispose();
			mLcd = null;
		}
	}
}
