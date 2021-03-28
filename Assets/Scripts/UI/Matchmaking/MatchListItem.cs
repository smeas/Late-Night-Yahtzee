using System;
using Matchmaking;
using TMPro;
using UnityEngine;

namespace UI.Matchmaking {
	// TODO: Display player count.
	public class MatchListItem : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI idText;
		[SerializeField] private TextMeshProUGUI hostText;
		[SerializeField] private TextMeshProUGUI createdText;
		[SerializeField] private TextMeshProUGUI playersText;

		private MatchmakingMenu matchmakingMenu;
		private MatchInfo matchInfo;

		public string Id => matchInfo.id;

		public void Initialize(MatchmakingMenu menu, MatchInfo match, string hostName) {
			matchmakingMenu = menu;
			hostText.text = hostName;
			UpdateMatch(match);
		}

		public void UpdateMatch(MatchInfo match) {
			matchInfo = match;
			idText.text = match.id;
			playersText.text = $"({match.playerCount}/{MatchmakingManager.MaxPlayers})";
			createdText.text = CreationTimeToString(match.CreatedTime);
		}

		public void UpdateLiveElements() {
			createdText.text = CreationTimeToString(matchInfo.CreatedTime);
		}

		public void Join() {
			matchmakingMenu.JoinMatch(Id);
		}

		private static string CreationTimeToString(DateTimeOffset creationTime) {
			TimeSpan timeSpan = DateTimeOffset.Now - creationTime;

			if (timeSpan.TotalSeconds < 45)
				return "less than a minute ago";

			if (timeSpan.TotalMinutes < 45)
				return timeSpan.TotalMinutes < 1.5
					? "a minute ago"
					: $"{timeSpan.TotalMinutes:0} minutes ago";

			if (timeSpan.TotalHours < 45)
				return timeSpan.TotalHours < 1.5
					? "an hour ago"
					: $"{timeSpan.TotalHours:0} hours ago";

			if (timeSpan.TotalDays < 365)
				return timeSpan.TotalDays < 1.5
					? "a day ago"
					: $"{timeSpan.TotalDays:0} days ago";

			return "a long time ago";
		}
	}
}