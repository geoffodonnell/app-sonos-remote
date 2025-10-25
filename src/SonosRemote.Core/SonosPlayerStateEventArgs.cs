using System;

namespace SonosRemote.Core {
	public class SonosPlayerStateEventArgs : EventArgs {

		public string ID => State.ID;

		public SonosPlayerState State { get; }

		public EventAction Action { get; }

		public SonosPlayerStateEventArgs(SonosPlayerState state, EventAction action) {
			State = state;
			Action = action;
		}
	}
}
