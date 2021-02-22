using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

namespace Matchmaking {
	// TODO: See if this needs to be a MonoBehaviour or not.
	public static class MatchmakingManager {
		private const int MaxQueryMatches = 10;


		public static DatabaseReference GamesReference => FirebaseManager.Instance.RootReference.Child("games");

		public static Query OpenMatchesQuery => GamesReference
			//.OrderByChild("created") // Sort the matches oldest first
			.OrderByChild("active").EqualTo(false)
			.LimitToFirst(MaxQueryMatches);


		public static async Task<MatchData> CreateMatch() {
			MatchData matchData = new MatchData {
				player1 = FirebaseManager.Instance.UserId,
				player2 = null,
				active = false,
				CreatedTime = DateTimeOffset.UtcNow,
			};

			DatabaseReference matchReference = GamesReference.Push();
			matchData.id = matchReference.Key;
			await matchReference.SetRawJsonValueAsync(JsonUtility.ToJson(matchData));

			Debug.Log($"Created match with id: {matchData.id}");

			return matchData;
		}

		public static async void DeleteMatch(string id) {
			await GamesReference.Child(id).RemoveValueAsync();
		}

		/// <returns>Match data if successful, null otherwise.</returns>
		public static async Task<MatchData> TryJoinMatch(string matchId) {
			DatabaseReference matchReference = GamesReference.Child(matchId);

			// Run a transaction to avoid any potential race conditions.
			DataSnapshot dataSnapshot = await matchReference.RunTransaction(data => {
				if (data == null)
					return TransactionResult.Success(null);

				if ((bool)data.Child(nameof(MatchData.active)).Value) {
					// Someone else already joined.
					return TransactionResult.Abort();
				}

				// Write the data.
				data.Child(nameof(MatchData.active)).Value = true;
				data.Child(nameof(MatchData.player2)).Value = FirebaseManager.Instance.UserId;

				return TransactionResult.Success(data);
			});

			if (dataSnapshot == null)
				return null;

			MatchData matchData = SnapshotToMatchData(dataSnapshot);
			if (matchData.active && matchData.player2 == FirebaseManager.Instance.UserId)
				return matchData;

			return null;
		}

		public static async Task<MatchData[]> GetOpenMatches() {
			DataSnapshot dataSnapshot = await OpenMatchesQuery.GetValueAsync();

			MatchData[] matches = dataSnapshot.Children.Select(SnapshotToMatchData).ToArray();

			return matches;
		}

		public static MatchData SnapshotToMatchData(DataSnapshot snap) {
			MatchData matchData = JsonUtility.FromJson<MatchData>(snap.GetRawJsonValue());
			matchData.id = snap.Key;
			return matchData;
		}
	}
}