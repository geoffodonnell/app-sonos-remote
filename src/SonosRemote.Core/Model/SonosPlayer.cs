using OpenSource.UPnP;
using System;
using System.Net;

namespace SonosRemote.Core.Model {
	public class SonosPlayer : IEquatable<SonosPlayer> {

		public string ID => UniqueDeviceName;

		/// <summary>
		/// Unique Device Name
		/// </summary>
		/// <example>
		/// "RINCON_5CAAFD9AFD4001400"
		/// </example>
		public string UniqueDeviceName { get; set; }

		/// <summary>
		/// Base URL
		/// </summary>
		/// <example>
		/// "http://10.0.1.38:1400/xml"
		/// </example>
		public Uri BaseUrl { get; set; }

		/// <summary>
		/// Location URL
		/// </summary>
		/// <example>
		/// "http://10.0.1.38:1400/xml/device_description.xml"
		/// </example>
		public string LocationUrl { get; set; }

		/// <summary>
		/// Manufacturer
		/// </summary>
		/// <example>
		/// "Sonos, Inc."
		/// </example>
		public string Manufacturer { get; set; }

		/// <summary>
		/// Friendly Name
		/// </summary>
		/// <example>
		/// "10.0.1.92 - Sonos PLAY:1"
		/// </example>
		public string FriendlyName { get; set; }

		/// <summary>
		/// Model Description
		/// </summary>
		/// <example>
		/// "Sonos PLAY:1"
		/// </example>
		public string ModelDescription { get; set; }

		/// <summary>
		/// Model Name
		/// </summary>
		/// <example>
		/// "Sonos PLAY:1"
		/// </example>
		public string ModelName { get; set; }

		/// <summary>
		/// Model Number
		/// </summary>
		/// <example>
		/// "S1"
		/// </example>
		public string ModelNumber { get; set; }

		/// <summary>
		/// Standard Device Type
		/// </summary>
		/// <example>
		/// "ZonePlayer" || "MediaRenderer" || "MediaServer"
		/// </example>
		public string StandardDeviceType { get; set; }

		/// <summary>
		/// Remote End Point
		/// </summary>
		/// <example
		/// {10.0.1.92:1400}
		/// </example>
		public IPEndPoint RemoteEndPoint { get; set; }

		/// <summary>
		/// Device
		/// </summary>
		public UPnPDevice Device { get; set; }
		
		public override string ToString() {
			return FriendlyName ?? string.Empty;
		}

		public override bool Equals(object obj) {

			return Equals(obj as SonosPlayer);
		}

		public override int GetHashCode() {

			var result = new HashCode();

			result.Add(UniqueDeviceName);
			result.Add(BaseUrl);
			result.Add(LocationUrl);
			result.Add(FriendlyName);
			result.Add(ModelNumber);
			result.Add(ModelDescription);
			result.Add(ModelName);
			result.Add(Manufacturer);
			result.Add(RemoteEndPoint);
			result.Add(StandardDeviceType);
			result.Add(Device);

			return result.ToHashCode();
		}

		public bool Equals(SonosPlayer other) {

			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(SonosPlayer obj1, SonosPlayer obj2) {

			if (ReferenceEquals(obj1, obj2)) {
				return true;
			}

			if (ReferenceEquals(obj1, null)) {
				return false;
			}

			if (ReferenceEquals(obj2, null)) {
				return false;
			}

			return obj1.Equals(obj2);
		}

		public static bool operator !=(SonosPlayer obj1, SonosPlayer obj2) {

			return !(obj1 == obj2);
		}

		public static SonosPlayer CreateFromUpnpDevice(UPnPDevice device) {

			return new SonosPlayer {
				UniqueDeviceName = device.UniqueDeviceName,
				BaseUrl = device.BaseURL,
				LocationUrl = device.LocationURL,
				FriendlyName = device.FriendlyName,
				ModelNumber = device.ModelNumber,
				ModelDescription = device.ModelDescription,
				ModelName = device.ModelName,
				Manufacturer = device.Manufacturer,
				RemoteEndPoint = device.RemoteEndPoint,
				StandardDeviceType = device.StandardDeviceType,
				Device = device
			};
		}
	}
}