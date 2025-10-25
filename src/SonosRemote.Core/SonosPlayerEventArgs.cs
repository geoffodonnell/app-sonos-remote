using SonosRemote.Core.Model;
using System;

namespace SonosRemote.Core {
	public class SonosPlayerEventArgs : EventArgs {

		public string ID => Player.ID;

		public SonosPlayer Player { get; }

		public EventAction Action { get; }

		public SonosPlayerEventArgs(SonosPlayer player, EventAction action) {
			Player = player;
			Action = action;
		}
	}
}
