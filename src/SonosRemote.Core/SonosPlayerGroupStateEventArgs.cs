using System;

namespace SonosRemote.Core {
	public class SonosPlayerGroupStateEventArgs : EventArgs {

		public string ID => State.ID;

		public SonosPlayerGroupState State { get; }

		public EventAction Action { get; }

		public SonosPlayerGroupStateEventArgs(SonosPlayerGroupState state, EventAction action) {
			State = state;
			Action = action;
		}
	}
}
