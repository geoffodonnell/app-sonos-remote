using SonosRemote.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace SonosRemote.Core {
	public class SonosPlayerGroupState : IEquatable<SonosPlayerGroupState> {

		public string ID => Group?.Master?.ID;

		public DateTimeOffset Created { get; set; }

		public DateTimeOffset Updated { get; set; }

		public DateTimeOffset Changed { get; set; }

		public SonosPlayerGroup Group { get; set; }

		public SonosPlayerState MasterState { get; set; }

		public List<SonosPlayerState> MemberStates { get; set; }

		public override bool Equals(object obj) {

			return Equals(obj as SonosPlayerGroupState);
		}

		public override int GetHashCode() {

			var result = new HashCode();

			result.Add(Group);
			result.Add(MasterState);

			if (MemberStates != null) {
				foreach (var item in MemberStates.OrderBy(s => s.ID, StringComparer.InvariantCultureIgnoreCase)) {
					result.Add(item);
				}
			}

			return result.ToHashCode();
		}

		public bool Equals(SonosPlayerGroupState other) {

			if (ReferenceEquals(other, null)) {
				return false;
			}

			if (ReferenceEquals(this, other)) {
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public static bool operator ==(SonosPlayerGroupState obj1, SonosPlayerGroupState obj2) {

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

		public static bool operator !=(SonosPlayerGroupState obj1, SonosPlayerGroupState obj2) {

			return !(obj1 == obj2);
		}
	}
}
