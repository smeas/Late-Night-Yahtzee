using System;
using Extensions;
using Yahtzee;

namespace Matchmaking {
	[Serializable]
	public class GameInfo : DataWithId {
		public int playerCount;
		public string[] players;
		public PlayerState[] states;
		public int turn;
	}
}