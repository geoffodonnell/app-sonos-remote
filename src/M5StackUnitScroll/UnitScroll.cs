using System;
using System.Threading;
using System.Threading.Tasks;

namespace M5StackUnitScroll {
	public class UnitScroll : IDisposable {

		public UnitScrollDriver Driver { get; protected set; }

		public event EventHandler<ButtonEventArgs> ButtonChanged;
		public event EventHandler<EncoderEventArgs> EncoderChanged;
		public event EventHandler<LedEventArgs> LedChanged;

		protected CancellationTokenSource CancellationTokenSource { get; private set; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		private bool? mButtonValue = null;
		private short? mEncoderValue = null;
		private byte[] mLedValue = null;

		private bool mDisposedValue;

		public UnitScroll(UnitScrollDriver driver) {
			
			Driver = driver;
			CancellationTokenSource = new CancellationTokenSource();

			var thread = new Thread(MonitorThread) {
				IsBackground = true
			};

			thread.Start();
		}

		public UnitScroll()
			: this(new UnitScrollDriver()) { }

		public UnitScroll(int busId, int deviceAddress)
			: this(new UnitScrollDriver(busId, deviceAddress)) { }


		/// <summary>
		/// Get the current button state
		/// </summary>
		/// <returns>True if pressed, False otherwise</returns>
		public virtual bool GetButtonValue() {

			if (!mButtonValue.HasValue) {
				ReadButtonValue();
			}

			return mButtonValue.HasValue && mButtonValue.Value;
		}

		public virtual short GetEncoderValue() {

			if (!mEncoderValue.HasValue) {
				ReadEncoderValue();
			}

			return mEncoderValue.HasValue ? mEncoderValue.Value : (short)0;
		}

		public virtual byte[] GetLedValue() {

			if (mLedValue == null) {
				ReadLedValue();
			}

			return mLedValue;
		}

		public virtual short GetIncEncoderValue() {

			var valueAsBytes = Driver.ReadIncEncoderValue();
			var value = (short)((valueAsBytes[1] << 8) | valueAsBytes[0]);

			return value;
		}

		public virtual void SetEncoderValue(short value) {

			// Split the short into two bytes (little-endian)
			var lowByte = (byte)(value & 0xFF);
			var highByte = (byte)((value >> 8) & 0xFF);

			Driver.WriteEncoderValue([lowByte, highByte]);
		}

		public virtual void SetLedValue(byte r, byte g, byte b) {

			Driver.WriteLedState([0x00, r, g, b]);
		}

		public virtual void SetReset() {

			Driver.WriteReset();
		}

		protected virtual void MonitorThread() {

			while (!CancellationToken.IsCancellationRequested) {
				ReadButtonValue();
				ReadEncoderValue();
				ReadLedValue();

				Task.Delay(25, CancellationToken).Wait();
			}
		}

		protected virtual void ReadButtonValue() {

			var valueAsByte = Driver.ReadButtonValue();
			var value = valueAsByte == 0x01 ? false : true;

			if (!mButtonValue.HasValue || mButtonValue.Value != value) {
				mButtonValue = value;
				OnButtonChanged(new ButtonEventArgs(value));
			}
		}

		protected virtual void ReadEncoderValue() {

			var valueAsByte = Driver.ReadEncoderValue();
			var value = (short)((valueAsByte[1] << 8) | valueAsByte[0]);

			if (!mEncoderValue.HasValue || mEncoderValue.Value != value) {
				mEncoderValue = value;
				OnEncoderChanged(new EncoderEventArgs(value));
			}
		}

		protected virtual void ReadLedValue() {

			var valueAsBytes = Driver.ReadLedValue();
			var value = new byte[] { valueAsBytes[0], valueAsBytes[1], valueAsBytes[2], valueAsBytes[3] };

			if (mLedValue == null || 
				mLedValue[0] != value[0] || 
				mLedValue[1] != value[1] ||
				mLedValue[2] != value[2] ||
				mLedValue[3] != value[3]) {

				mLedValue = value;
				OnLedChanged(new LedEventArgs(value[1], value[2], value[3]));
			}
		}

		protected virtual void OnButtonChanged(ButtonEventArgs e) {
			ButtonChanged?.Invoke(this, e);
		}

		protected virtual void OnEncoderChanged(EncoderEventArgs e) {
			EncoderChanged?.Invoke(this, e);
		}

		protected virtual void OnLedChanged(LedEventArgs e) {
			LedChanged?.Invoke(this, e);
		}

		protected virtual void Dispose(bool disposing) {
			if (!mDisposedValue) {
				if (disposing) {
					CancellationTokenSource.Cancel();

					Driver.Dispose();
					CancellationTokenSource.Dispose();
				}

				Driver = null;
				CancellationTokenSource = null;
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
