using System;
using Extensions;
using Firebase.Database;

namespace Matchmaking {
	[Serializable]
	public class MatchInfo : DataWithId {
		public long created;
		public bool isStarting;
		public bool started;
		public int playerCount;
		public string[] players;

		public bool IsFull => playerCount >= MatchmakingManager.MaxPlayers;

		public DateTimeOffset CreatedTime {
			get => DateTimeOffset.FromUnixTimeSeconds(created);
			set => created = value.ToUnixTimeSeconds();
		}

		public DatabaseReference Reference => MatchmakingManager.MatchesReference.Child(id);
	}
}