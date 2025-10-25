namespace M5StackUnitScroll {
	public static class Registers {
		public const byte Encoder = 0x10;
		public const byte Button = 0x20;
		public const byte Led = 0x30;				// R/G/B
		public const byte Reset = 0x40;
		public const byte IncEncoder = 0x50;
		public const byte EncoderMode = 0xFB;		// Encoder AB or BA
		public const byte BootloaderVersion = 0xFC;
		public const byte FirmwareVersion = 0xFE;
		public const byte I2cAddress = 0xFF;
	}
}
