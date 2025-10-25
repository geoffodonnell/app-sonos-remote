using System.Drawing;

namespace M5StackUnitScroll {
	public static class Extensions {

		public static void SetLedValue(this UnitScroll device, Color color) {
			device.SetLedValue(color.R, color.G, color.B);
		}

		public static void SetLedOff (this UnitScroll device) {
			device.SetLedValue(0, 0, 0);
		}

		public static Color GetColor(this LedEventArgs e) {
			return Color.FromArgb(e.R, e.G, e.B);
		}

		public static string ToHexString(this Color value) {
			return $"#{value.R:X2} {value.G:X2} {value.B:X2}";
		}
	}
}
