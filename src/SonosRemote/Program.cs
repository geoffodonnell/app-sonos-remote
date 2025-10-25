using ByteDev.Sonos;
using ByteDev.Sonos.Device;
using ByteDev.Sonos.Upnp.Services;
using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using M5StackUnitScroll;
using SonosRemote.Core;
using SonosRemote.Core.Discovery;
using SonosRemote.Display;
using SonosRemote.Volume;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Linq;
using System.Threading.Tasks;

namespace SonosRemote {
	internal partial class Program {
		static void Main(string[] args) {

			try {
				if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
					MainWindows(args);
				} else {
					MainDevice(args);
				}
			} catch (Exception ex) {
				Console.WriteLine("An error occurred:");
				Console.WriteLine(ex.Message.ToString());
			}
		}

		private static void MainWindows(string[] args) {

			var volume = 1;
			using var playerManager = new SonosPlayerManager();
			using var groupManager = new SonosPlayerGroupManager(playerManager);
			using var displayManager = CreateDisplayManager();

			groupManager.PlayerGroupStateAdded += (s, e) => {
				displayManager.Track = e.State.MasterState.CurrentTrack.TrackMetaData.Title;
				displayManager.Volume = e.State.MasterState.Volume;
			};

			groupManager.PlayerGroupStateChanged += (s, e) => {
				displayManager.Track = e.State.MasterState.CurrentTrack.TrackMetaData.Title;
				displayManager.Volume = e.State.MasterState.Volume;
			};

			var togglePlay = () => {
				if (groupManager.Groups.Count == 0) {
					return;
				}
				var group = groupManager.Groups.First();
				groupManager.TogglePlayingAsync(group);				
			};

			var setVolume = (int v) => {
				displayManager.Volume = v;

				if (groupManager.Groups.Count == 0) {
					return;
				}
				var group = groupManager.Groups.First();
				groupManager.SetVolumeAsync(group, v);
			};

			displayManager.TurnOn();
			displayManager.Clear();
			displayManager.Track = "Searching...";
			playerManager.Start();

			while (true) {
				Task.Delay(50).Wait();

				if (Console.KeyAvailable) {
					var read = Console.ReadKey(true);

					if (read.Key == ConsoleKey.UpArrow) {
						volume++;
						setVolume(volume);
					} else if (read.Key == ConsoleKey.DownArrow) {
						volume--;
						setVolume(volume);
					} else if (read.Key == ConsoleKey.P || read.Key == ConsoleKey.Spacebar) {
						togglePlay();
					} else {
						break;
					}
				}
			}

			//while (!Console.KeyAvailable) {
			//	Task.Delay(50).Wait();
			//}

			while (Console.KeyAvailable) {
				Console.ReadKey();
			}
		}

		private static void MainDevice(string[] args) {

			using var unitScroll = CreateUnitScroll();
			using var playerManager = new SonosPlayerManager();
			using var groupManager = new SonosPlayerGroupManager(playerManager);
			using var volumeControl = new VolumeControl(unitScroll);
			using var displayManager = CreateDisplayManager();

			groupManager.PlayerGroupStateAdded += (s, e) => {
				displayManager.Track = e.State.MasterState.CurrentTrack.TrackMetaData.Title;
				displayManager.Volume = e.State.MasterState.Volume;
			};

			groupManager.PlayerGroupStateChanged += (s, e) => {
				displayManager.Track = e.State.MasterState.CurrentTrack.TrackMetaData.Title;
				displayManager.Volume = e.State.MasterState.Volume;
			};

			unitScroll.ButtonChanged += (s, e) => {
				if (!e.IsPressed) {
					if (groupManager.Groups.Count == 0) {
						return;
					}
					var group = groupManager.Groups.First();
					groupManager.TogglePlayingAsync(group);
				}
			};

			volumeControl.VolumeChanged += (s, e) => {
				displayManager.Volume = e.Volume;

				if (groupManager.Groups.Count == 0) {
					return;
				}
				var group = groupManager.Groups.First();
				groupManager.SetVolumeAsync(group, e.Volume);
			};

			displayManager.TurnOn();
			displayManager.Clear();
			displayManager.Track = "Searching...";
			playerManager.Start();

			while (!Console.KeyAvailable) {
				Task.Delay(50).Wait();
			}

			while (Console.KeyAvailable) {
				Console.ReadKey();
			}
		}

		private static UnitScroll CreateUnitScroll() {

			return new UnitScroll(1, 0x40);
		}

		private static DisplayManager CreateDisplayManager() {

			var display = CreateDisplay();

			return new DisplayManager(display, shouldDispose: true);
		}

		private static IDisplay CreateDisplay() {

			var consoleDisplay = new ConsoleDisplay(16, 2);

			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				return consoleDisplay;
			}

			var i2cSettings = new I2cConnectionSettings(busId: 1, deviceAddress: 0x27);
			var i2cDevice = I2cDevice.Create(i2cSettings);
			var driver = new Pcf8574(i2cDevice);
			var lcd = new Lcd1602(
				registerSelectPin: 0,
				enablePin: 2,
				dataPins: new int[] { 4, 5, 6, 7 },
				backlightPin: 3,
				readWritePin: 1,
				controller: new GpioController(driver));

			var lcdDisplay = new LcdDisplay(lcd);

			return new CombinedDisplay([lcdDisplay, consoleDisplay]);
		}
	}
}
