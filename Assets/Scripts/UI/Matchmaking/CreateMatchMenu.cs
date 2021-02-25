using Firebase.Database;
using Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Matchmaking {

	// TODO: Handle the case where the match is deleted just when someone joins.

	public class CreateMatchMenu : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI idText;
		[SerializeField] private SceneReference gameScene;

		private MatchData matchData;
		private DatabaseReference matchReference;

		private void OnDisable() {
			if (matchReference != null)
				matchReference.ValueChanged -= MatchReferenceOnValueChanged;
		}

		public async void ShowAndCreateMatch() {
			idText.text = "";
			gameObject.SetActive(true);

			matchData = await MatchmakingManager.CreateMatch();
			matchReference = MatchmakingManager.GamesReference.Child(matchData.id);

			// Delete the match when the game quits.
			await matchReference.OnDisconnect().RemoveValue();

			matchReference.ValueChanged += MatchReferenceOnValueChanged;

			idText.text = matchData.id;
		}

		public async void CreateMatch() {
			idText.text = "";

			matchData = await MatchmakingManager.CreateMatch();
			matchReference = MatchmakingManager.GamesReference.Child(matchData.id);

			// Delete the match when the game quits.
			await matchReference.OnDisconnect().RemoveValue();

			matchReference.ValueChanged += MatchReferenceOnValueChanged;

			idText.text = matchData.id;
		}

		public void DeleteMatch() {
			if (matchReference != null)
				matchReference.ValueChanged -= MatchReferenceOnValueChanged;

			MatchmakingManager.DeleteMatch(matchData.id);
		}

		private void MatchReferenceOnValueChanged(object sender, ValueChangedEventArgs e) {
			if ((bool)e.Snapshot.Child(nameof(MatchData.active)).Value) {
				// Someone joined the match.
				// TODO: Figure out if we want to remove the onDisconnect operation here.

				SceneManager.LoadScene(gameScene);

				//throw new NotImplementedException("Handle join");
			}
		}

		public void HideAndDeleteMatch() {
			gameObject.SetActive(false);

			MatchmakingManager.DeleteMatch(matchData.id);
		}
	}
}