using System.Drawing;

namespace SonosRemote.Display {
	public interface IDisplay {

		public Size Size { get; }

		public void TurnOn();

		public void TurnOff();

		public void Clear();

		public void Write(string text);

		public void SetCursorPosition(int left, int top);
	}
}
