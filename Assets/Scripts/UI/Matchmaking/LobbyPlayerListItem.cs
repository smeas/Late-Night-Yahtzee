using TMPro;
using UnityEngine;

namespace UI.Matchmaking {
	public class LobbyPlayerListItem : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI usernameText;

		private UserInfo info;

		public string PlayerId => info.id;

		public void Initialize(UserInfo userInfo) {
			info = userInfo;
			usernameText.text = userInfo.username;
		}
	}
}