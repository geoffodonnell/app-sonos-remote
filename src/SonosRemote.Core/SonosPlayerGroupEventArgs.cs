using SonosRemote.Core.Model;
using System;

namespace SonosRemote.Core {
	public class SonosPlayerGroupEventArgs : EventArgs {

		public string ID => Group.ID;

		public SonosPlayerGroup Group { get; }

		public EventAction Action { get; }

		public SonosPlayerGroupEventArgs(SonosPlayerGroup group, EventAction action) {
			Group = group;
			Action = action;
		}
	}
}
