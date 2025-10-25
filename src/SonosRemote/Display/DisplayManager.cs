using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SonosRemote.Display {

	public class DisplayManager : IDisposable {

		protected static readonly object mWriteLock = new object();

		public string Track {
			get => mTrack;
			set {
				mTrack = value;
				HandleTrackChanged();
			}
		}

		public int Volume {
			get => mVolume;
			set {
				mVolume = value;
				HandleVolumeChanged();
			}
		}

		protected IDisplay Display { get; private set; }
		protected bool ShouldDispose { get; private set; }

		protected string[] CurrentLines { get; private set; }
		protected int[] CurrentPositions { get; private set; }

		protected CancellationTokenSource CancellationTokenSource { get; private set; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		private string mTrack;
		private int mVolume;
		private bool mDisposedValue;

		public DisplayManager(IDisplay display, bool shouldDispose = false) {

			if (display == null) {
				throw new ArgumentNullException(nameof(display));
			}

			Display = display;
			ShouldDispose = shouldDispose;

			CurrentLines = new string[display.Size.Height];
			CurrentPositions = new int[display.Size.Height];

			CancellationTokenSource = new CancellationTokenSource();

			var thread = new Thread(PositionsThread) {
				IsBackground = true
			};

			thread.Start();
		}

		public virtual void TurnOn() {
			Display.TurnOn();
		}

		public virtual void TurnOff() {
			Display.TurnOff();
		}

		public virtual void Clear() {
			Clear(null);
		}

		protected virtual void HandleTrackChanged() {
		
			if (Display.Size.Height > 0) {
				if (String.IsNullOrWhiteSpace(Track)) {
					Clear(0);
				} else if (Track.Length > Display.Size.Width) {

					var track = new StringBuilder();
					var space = new string(' ', Display.Size.Width / 2);

					track.Append(space);
					track.Append(Track);
					track.Append(space);
					track.Append(space);

					Write(0, track.ToString());
				} else {
					Write(0, Track);
				}
			}
		}

		protected virtual void HandleVolumeChanged() {

			if (Display.Size.Height > 1) {
				Write(1, $"Volume: {Volume}".PadRight(Display.Size.Width));
			}
		}

		protected virtual void Write(int line, string text) {

			if (line >= Display.Size.Height) {
				throw new ArgumentOutOfRangeException(nameof(line), $"Line must be between 0 and {Display.Size.Height - 1}");
			}

			CurrentLines[line] = text;
			CurrentPositions[line] = 0;

			UpdateLines();
		}

		protected virtual void Clear(int? line) {

			if (line.HasValue) {
				Write(line.Value, new string(' ', Display.Size.Width));
			} else {
				Array.Clear(CurrentLines, 0, CurrentLines.Length);
				Array.Clear(CurrentPositions, 0, CurrentPositions.Length);
				Display.Clear();
			}
		}

		protected virtual void UpdateLines() {

			lock (mWriteLock) {
				for (int i = 0; i < CurrentLines.Length; i++) {

					var line = CurrentLines[i];
					var position = CurrentPositions[i];

					if (line == null) {
						continue;
					}

					if (line.Length <= Display.Size.Width) {
						Display.SetCursorPosition(0, i);
						Display.Write(line.PadRight(Display.Size.Width));
					} else {
						var displayText = line.Substring(
							position, Math.Min(Display.Size.Width, line.Length - position));
						Display.SetCursorPosition(0, i);
						Display.Write(displayText.PadRight(Display.Size.Width));
					}
				}
			}
		}

		protected virtual void UpdatePositions() {

			for (int i = 0; i < CurrentLines.Length; i++) {

				var line = CurrentLines[i];
				var position = CurrentPositions[i];

				if (line == null) {
					continue;
				}

				if (line.Length > Display.Size.Width) {
					position++;
					if (position + Display.Size.Width > line.Length) {
						position = 0;
					}
					CurrentPositions[i] = position;
				}
			}

			UpdateLines();
		}

		private void PositionsThread() {

			while (!CancellationToken.IsCancellationRequested) {
				UpdatePositions();

				Task.Delay(500, CancellationToken).Wait();
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (!mDisposedValue) {
				if (disposing) {
					CancellationTokenSource.Cancel();
					CancellationTokenSource.Dispose();

					if (ShouldDispose) {
						Display.Clear();
						Display.TurnOff();
						(Display as IDisposable)?.Dispose();
					}
				}

				Display = null;
				CurrentLines = null;
				CurrentPositions = null;
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
