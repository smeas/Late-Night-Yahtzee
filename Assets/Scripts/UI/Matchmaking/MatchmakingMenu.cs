using System.Collections.Generic;
using Firebase.Database;
using Matchmaking;
using UnityEngine;

namespace UI.Matchmaking {
	public class MatchmakingMenu : MonoBehaviour {
		[SerializeField] private MatchListItem matchListItemPrefab;
		[SerializeField] private Transform matchListRoot;

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
				Debug.Log($"Successfully joined match: {id}.");
			}
			else {
				Debug.Log($"Failed to join match: {id}.");
			}

			isJoiningMatch = false;
		}

		private void MatchesQueryOnChildAdded(object sender, ChildChangedEventArgs e) {
			AddMatch(MatchmakingManager.SnapshotToMatchData(e.Snapshot));
		}

		private void MatchesQueryOnChildRemoved(object sender, ChildChangedEventArgs e) {
			RemoveMatch(e.Snapshot.Key);
		}

		private void AddMatch(MatchData match) {
			MatchListItem item = Instantiate(matchListItemPrefab, matchListRoot);
			item.Initialize(this, match.id, match.player1);
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