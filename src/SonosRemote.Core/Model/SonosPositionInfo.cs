using ByteDev.Sonos.Upnp.Services.Models;
using System;

namespace SonosRemote.Core.Model {
	public class SonosPositionInfo : IEquatable<SonosPositionInfo> {

		public int TrackNumber { get; set; }

		public TimeSpan TrackDuration { get; set; }

		public string TrackMetaDataRaw { get; set; }

		public SonosTrackMetaData TrackMetaData { get; set; }

		public string TrackUri { get; set; }

		public TimeSpan RelativeTime { get; set; }

		public string AbsoluteTime { get; set; }

		public int RelativeCount { get; set; }

		public int AbsoluteCount { get; set; }

		public override bool Equals(object obj) {

			return Equals(obj as SonosPositionInfo);
		}

		public override int GetHashCode() {

			// Purposely left TrackMetaDataRaw out, if a difference there doesn't go into
			// TrackMetaData isn't probably not worth caring about.
			return HashCode.Combine(
				TrackNumber,
				TrackDuration,
				TrackMetaData,
				TrackUri,
				RelativeTime,
				AbsoluteCount,
				RelativeCount,
				AbsoluteCount);
		}

		public bool Equals(SonosPositionInfo other) {

			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(SonosPositionInfo obj1, SonosPositionInfo obj2) {

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

		public static bool operator !=(SonosPositionInfo obj1, SonosPositionInfo obj2) {

			return !(obj1 == obj2);
		}

		public static SonosPositionInfo Create(GetPositionInfoResponse response) {
			return new SonosPositionInfo {
				TrackNumber = response.TrackNumber,
				TrackDuration = response.TrackDuration,
				TrackMetaDataRaw = response.TrackMetaDataRaw,
				TrackMetaData = SonosTrackMetaData.Create(response.TrackMetaData),
				TrackUri = response.TrackUri,
				RelativeTime = response.RelativeTime,
				AbsoluteTime = response.AbsoluteTime,
				RelativeCount = response.RelativeCount,
				AbsoluteCount = response.AbsoluteCount
			};
		}
	}
}
