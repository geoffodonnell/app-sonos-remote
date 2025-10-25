using ByteDev.Sonos.Upnp.Services.Models;
using System;

namespace SonosRemote.Core.Model {
	public class SonosTrackMetaData : IEquatable<SonosTrackMetaData> {

		public string ProtocolInfo { get; set; }

		public string Duration { get; set; }

		public string AlbumArtUri { get; set; }

		public string Title { get; set; }

		public string Class { get; set; }

		public string Creator { get; set; }

		public string Album { get; set; }

		public string Res { get; set; }

		public override bool Equals(object obj) {

			return Equals(obj as SonosTrackMetaData);
		}

		public override int GetHashCode() {

			return HashCode.Combine(ProtocolInfo, Duration, AlbumArtUri, Title, Class, Creator, Album, Res);
		}

		public bool Equals(SonosTrackMetaData other) {

			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(SonosTrackMetaData obj1, SonosTrackMetaData obj2) {

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

		public static bool operator !=(SonosTrackMetaData obj1, SonosTrackMetaData obj2) {

			return !(obj1 == obj2);
		}

		public static SonosTrackMetaData Create(TrackMetaData value) {
			return new SonosTrackMetaData {
				ProtocolInfo = value.ProtocolInfo,
				Duration = value.Duration,
				AlbumArtUri = value.AlbumArtUri,
				Title = value.Title,
				Class = value.Class,
				Creator = value.Creator,
				Album = value.Album,
				Res = value.Res
			};
		}
	}
}
