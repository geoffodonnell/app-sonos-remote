using System;
using System.Device.I2c;

namespace M5StackUnitScroll {

	/// <summary>
	/// M5Stack Unit Scroll I2C Driver 
	/// </summary>
	/// <seealso cref="https://docs.m5stack.com/en/unit/UNIT-Scroll"/>
	/// <seealso cref="https://github.com/m5stack/M5Unit-Scroll-Internal-FW/blob/main/code/APP/Encoder_EVQ/Core/Src/main.c"/>
	public class UnitScrollDriver : IDisposable {

		public I2cDevice I2cDevice { get; protected set; }

		private bool mDisposedValue;

		public UnitScrollDriver(I2cDevice i2cDevice) {

			I2cDevice = i2cDevice;
		}

		public UnitScrollDriver(int busId = 1, int deviceAddress = 0x40)
			: this(I2cDevice.Create(new I2cConnectionSettings(busId, deviceAddress))) { }

		public virtual byte[] ReadEncoderValue() {

			var buffer = new byte[2];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.Encoder }), target);

			return buffer;
		}

		public virtual byte ReadButtonValue() {

			var buffer = new byte[1];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.Button }), target);
			
			return buffer[0];			
		}

		public virtual byte[] ReadLedValue() {

			var buffer = new byte[4];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.Led }), target);

			return [buffer[0], buffer[1], buffer[2], buffer[3]];
		}

		public virtual byte[] ReadIncEncoderValue() {

			var buffer = new byte[2];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.IncEncoder }), target);

			return buffer;
		}

		public virtual byte ReadEncoderModeValue() {

			var buffer = new byte[1];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.EncoderMode }), target);

			return buffer[0];
		}

		public virtual byte ReadBootloaderVersion() {

			var buffer = new byte[1];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.BootloaderVersion }), target);

			return buffer[0];
		}

		public virtual byte ReadFirmwareVersion() {

			var buffer = new byte[1];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.FirmwareVersion }), target);

			return buffer[0];
		}

		public virtual byte ReadI2cAddress() {

			var buffer = new byte[1];
			var target = new Span<byte>(buffer);

			I2cDevice.WriteRead(new ReadOnlySpan<byte>(new byte[] { Registers.I2cAddress }), target);

			return buffer[0];
		}

		public virtual void WriteEncoderValue(byte[] value) {

			if (value == null) {
				throw new ArgumentNullException("value");
			}

			if (value.Length != 2) {
				throw new ArgumentException("value must contain 2 bytes");
			}

			I2cDevice.Write(new Span<byte>([ Registers.Encoder, value[0], value[1] ]));
		}

		public virtual void WriteLedState(byte[] value) {

			if (value == null) {
				throw new ArgumentNullException("value");
			}

			if (value.Length != 3 && value.Length != 4) {
				throw new ArgumentException("value must contain 3 or 4 bytes");
			}

			var buffer = value.Length == 3 
				? [ (byte)0x00, value[0], value[1], value[2] ]
				: value;

			I2cDevice.Write(new Span<byte>([Registers.Led, buffer[0], buffer[1], buffer[2], buffer[3]]));
		}

		public virtual void WriteReset() {

			I2cDevice.Write(new Span<byte>([ Registers.Reset, 0x01 ]));
		}

		public virtual void WriteEncoderMode(byte value) {

			if (value != 0x00 || value != 0x01) {
				throw new ArgumentOutOfRangeException("value must be 0x00 or 0x01");
			}

			I2cDevice.Write(new Span<byte>([ Registers.EncoderMode, value ]));
		}

		protected virtual void Dispose(bool disposing) {
			if (!mDisposedValue) {
				if (disposing) {
					I2cDevice.Dispose();
				}

				I2cDevice = null;
				mDisposedValue = true;
			}
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
