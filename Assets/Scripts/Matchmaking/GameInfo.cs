using System;
using Extensions;
using Firebase.Database;
using Yahtzee;

namespace Matchmaking {
	[Serializable]
	public class GameInfo : DataWithId {
		public int playerCount;
		public string[] players;
		public PlayerState[] states;
		public int turn;

		public DatabaseReference Reference => MatchmakingManager.GamesReference.Child(id);
	}
}