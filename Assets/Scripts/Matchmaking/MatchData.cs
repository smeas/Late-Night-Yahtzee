using System;
using Yahtzee;

namespace Matchmaking {
	[Serializable]
	public class MatchData {
		[NonSerialized]
		public string id;
		public string player1;
		public string player2;
		public bool active;
		public long created;
		public GameState state;

		public DateTimeOffset CreatedTime {
			get => DateTimeOffset.FromUnixTimeSeconds(created);
			set => created = value.ToUnixTimeSeconds();
		}
	}
}