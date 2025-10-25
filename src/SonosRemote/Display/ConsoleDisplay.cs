using System;
using System.Drawing;

namespace SonosRemote.Display {
	public class ConsoleDisplay : IDisplay {

		public Size Size { get; private set; }

		public ConsoleDisplay(int width, int height) {
			Size = new Size(width, height);
		}

		public void TurnOn() {
			
			Console.CursorVisible = false;
		}

		public void TurnOff() {

			Console.CursorVisible = true;
		}

		public void Clear() {
			Console.Clear();
		}

		public void Write(string text) {
			Console.Write(text);
		}

		public void SetCursorPosition(int left, int top) {
			Console.SetCursorPosition(left, top);
		}
	}
}
