using System.Collections.Generic;
using Firebase.Database;
using Matchmaking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Matchmaking {
	public class MatchmakingMenu : MonoBehaviour {
		[SerializeField] private MatchListItem matchListItemPrefab;
		[SerializeField] private Transform matchListRoot;
		[SerializeField] private SceneReference gameScene;

		private readonly List<MatchListItem> matchListItems = new List<MatchListItem>();
		private Query matchesQuery;
		private bool isJoiningMatch;

		private async void OnEnable() {
			await FirebaseManager.Instance.WaitForInitialization();
			if (!enabled)
				return;

			// Clear the list.
			matchListItems.Clear();
			foreach (Transform item in matchListRoot)
				Destroy(item.gameObject);

			matchesQuery = MatchmakingManager.OpenMatchesQuery;
			matchesQuery.ChildAdded += MatchesQueryOnChildAdded;
			matchesQuery.ChildRemoved += MatchesQueryOnChildRemoved;
		}

		private void OnDisable() {
			matchesQuery.ChildAdded -= MatchesQueryOnChildAdded;
			matchesQuery.ChildRemoved -= MatchesQueryOnChildRemoved;
		}

		public async void JoinMatch(string id) {
			if (isJoiningMatch)
				return;

			isJoiningMatch = true;

			MatchData matchData = await MatchmakingManager.TryJoinMatch(id);
			if (matchData != null) {
				Debug.Log($"[MM] Successfully joined match: {id}.");

				SceneManager.LoadScene(gameScene);
			}
			else {
				Debug.Log($"[MM] Failed to join match: {id}.");
			}

			isJoiningMatch = false;
		}

		private void MatchesQueryOnChildAdded(object sender, ChildChangedEventArgs e) {
			MatchData matchData = MatchmakingManager.SnapshotToMatchData(e.Snapshot);
			if (matchData.player1 == FirebaseManager.Instance.UserId)
				return;

			AddMatch(matchData);
		}

		private void MatchesQueryOnChildRemoved(object sender, ChildChangedEventArgs e) {
			RemoveMatch(e.Snapshot.Key);
		}

		private async void AddMatch(MatchData match) {
			MatchListItem item = Instantiate(matchListItemPrefab, matchListRoot);

			DataSnapshot usernameSnapshot = await FirebaseManager.Instance.RootReference
				.Child("users/" + match.player1)
				.Child(nameof(UserInfo.username))
				.GetValueAsync();

			string hostUsername = (string)usernameSnapshot?.GetValue(false);
			if (string.IsNullOrEmpty(hostUsername))
				hostUsername = match.player1;

			item.Initialize(this, match.id, hostUsername);

			matchListItems.Add(item);
		}

		private void RemoveMatch(string id) {
			int itemIndex = matchListItems.FindIndex(item => item.Id == id);
			if (itemIndex == -1)
				return;

			Destroy(matchListItems[itemIndex].gameObject);
			matchListItems.RemoveAt(itemIndex);
		}
	}
}