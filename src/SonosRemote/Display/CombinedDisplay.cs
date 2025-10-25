using System;
using System.Drawing;
using System.Linq;

namespace SonosRemote.Display {
	public class CombinedDisplay : IDisplay, IDisposable {

		protected IDisplay[] Displays { get; }

		public Size Size { get; private set; }

		public CombinedDisplay(IDisplay[] displays) {
			
			var width = displays.Min(d => d.Size.Width);
			var height = displays.Min(d => d.Size.Height);

			Displays = displays.ToArray();
			Size = new Size(width, height);
		}

		public void TurnOn() {

			foreach (var display in Displays) { display.TurnOn(); }
		}

		public void TurnOff() {

			foreach (var display in Displays) { display.TurnOff(); }
		}

		public void Clear() {

			foreach (var display in Displays) { display.Clear(); }
		}

		public void Write(string text) {

			foreach (var display in Displays) { display.Write(text); }
		}

		public void SetCursorPosition(int left, int top) {

			foreach (var display in Displays) { display.SetCursorPosition(left, top); }
		}

		public void Dispose() {

			foreach (var display in Displays) { (display as IDisposable)?.Dispose(); }
		}
	}
}
