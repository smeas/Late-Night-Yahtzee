using System;

namespace Matchmaking {
	[Serializable]
	public class MatchData {
		[NonSerialized]
		public string id;
		public string player1;
		public string player2;
		public bool active;
		public long created;

		// TODO: Add game data field.

		public DateTimeOffset CreatedTime {
			get => DateTimeOffset.FromUnixTimeSeconds(created);
			set => created = value.ToUnixTimeSeconds();
		}
	}
}