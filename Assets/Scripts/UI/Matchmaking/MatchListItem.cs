using TMPro;
using UnityEngine;

namespace UI.Matchmaking {
	// TODO: Display player count.
	public class MatchListItem : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI idText;
		[SerializeField] private TextMeshProUGUI hostText;

		private MatchmakingMenu matchmakingMenu;
		private string matchId;

		public string Id => matchId;

		public void Initialize(MatchmakingMenu menu, string id, string hostName) {
			matchmakingMenu = menu;
			matchId = id;
			idText.text = id;
			hostText.text = hostName;
		}

		public void Join() {
			matchmakingMenu.JoinMatch(matchId);
		}
	}
}