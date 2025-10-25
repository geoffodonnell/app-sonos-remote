using System;
using System.Collections.Generic;
using System.Linq;

namespace SonosRemote.Core.Model {
	public class SonosPlayerGroup : IEquatable<SonosPlayerGroup> {

		public string ID => Master?.UniqueDeviceName;

		public SonosPlayer Master { get; set; }

		public List<SonosPlayer> Members { get; set; }

		public override bool Equals(object obj) {

			return Equals(obj as SonosPlayerGroup);
		}

		public override int GetHashCode() {
			
			var result = new HashCode();

			result.Add(Master);

			if (Members != null) {
				foreach (var item in Members.OrderBy(s => s.ID, StringComparer.InvariantCultureIgnoreCase)) {
					result.Add(item);
				}
			}

			return result.ToHashCode();
		}

		public bool Equals(SonosPlayerGroup other) {

			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(SonosPlayerGroup obj1, SonosPlayerGroup obj2) {

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

		public static bool operator !=(SonosPlayerGroup obj1, SonosPlayerGroup obj2) {

			return !(obj1 == obj2);
		}
	}
}
