using System.Collections.Generic;
using System.Threading.Tasks;
using Extensions;
using Firebase.Database;
using Matchmaking;
using UnityEngine;

namespace UI.Matchmaking {
	public class MatchmakingMenu : MonoBehaviour {
		[SerializeField] private MatchListItem matchListItemPrefab;
		[SerializeField] private Transform matchListRoot;
		[SerializeField] private LobbyMenu lobbyMenu;

		private readonly List<MatchListItem> matchListItems = new List<MatchListItem>();
		private DatabaseReference matchesReference;

		private async void OnEnable() {
			await FirebaseManager.Instance.WaitForInitialization();
			if (!enabled)
				return;

			// Clear the list.
			matchListItems.Clear();
			foreach (Transform item in matchListRoot)
				Destroy(item.gameObject);

			// TODO: Use query to order by creation time.
			matchesReference = MatchmakingManager.MatchesReference;
			matchesReference.ChildAdded += MatchesOnChildAdded;
			matchesReference.ChildRemoved += MatchesOnChildRemoved;
			matchesReference.ChildChanged += MatchesOnChildChanged;
		}

		private void OnDisable() {
			matchesReference.ChildAdded -= MatchesOnChildAdded;
			matchesReference.ChildRemoved -= MatchesOnChildRemoved;
			matchesReference.ChildChanged -= MatchesOnChildChanged;
		}

		private void Update() {
			// Update every 5th second. This only works properly if we have a framerate > 1 fps
			if ((long)Time.unscaledTimeAsDouble % 5 == 0) {
				foreach (MatchListItem item in matchListItems) {
					item.UpdateLiveElements();
				}
			}
		}

		public void JoinMatch(string id) {
			lobbyMenu.Show();
			lobbyMenu.JoinMatch(id);
		}

		private void MatchesOnChildAdded(object sender, ChildChangedEventArgs e) {
			MatchInfo matchInfo = e.Snapshot.ToObjectWithId<MatchInfo>();
			Debug.Assert(matchInfo != null);
			Debug.Assert(matchInfo.players.Length == matchInfo.playerCount, "players.Length == playerCount");
			Debug.Assert(matchInfo.playerCount > 0, "playerCount > 0");
			Debug.Assert(matchInfo.playerCount <= MatchmakingManager.MaxPlayers, "playerCount <= MaxPlayers");

			AddMatch(matchInfo);
		}

		private void MatchesOnChildRemoved(object sender, ChildChangedEventArgs e) {
			RemoveMatch(e.Snapshot.Key);
		}

		private void MatchesOnChildChanged(object sender, ChildChangedEventArgs e) {
			string id = e.Snapshot.Key;
			int index = matchListItems.FindIndex(item => item.Id == id);
			if (index == -1)
				return;

			MatchInfo matchInfo = e.Snapshot.ToObjectWithId<MatchInfo>();
			if (matchInfo == null) {
				Debug.Assert(false);
				return;
			}

			matchListItems[index].UpdateMatch(matchInfo);
		}

		private async void AddMatch(MatchInfo match) {
			MatchListItem item = Instantiate(matchListItemPrefab, matchListRoot);

			string hostUsername = await GetHostUsername(match);
			item.Initialize(this, match, hostUsername);

			matchListItems.Add(item);
		}

		private void RemoveMatch(string id) {
			int itemIndex = matchListItems.FindIndex(item => item.Id == id);
			if (itemIndex == -1)
				return;

			Destroy(matchListItems[itemIndex].gameObject);
			matchListItems.RemoveAt(itemIndex);
		}

		private static async Task<string> GetHostUsername(MatchInfo match) {
			if (match.players.Length == 0) // should never happen
				return "";

			string hostUsername = await FirebaseManager.Instance.GetUsername(match.players[0]);
			if (string.IsNullOrEmpty(hostUsername)) // if username is blank, use id
				hostUsername = match.players[0];

			return hostUsername;
		}
	}
}