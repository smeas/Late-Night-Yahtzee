using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;
using Yahtzee;

namespace Matchmaking {
	// TODO: See if this needs to be a MonoBehaviour or not.
	public static class MatchmakingManager {
		private const int MaxQueryMatches = 10;


		public static DatabaseReference GamesReference => FirebaseManager.Instance.RootReference.Child("games");

		public static Query OpenMatchesQuery => GamesReference
			//.OrderByChild("created") // Sort the matches oldest first
			.OrderByChild("active").EqualTo(false)
			.LimitToFirst(MaxQueryMatches);

		public static MatchData CurrentMatch { get; private set; }


		public static async Task<MatchData> CreateMatch() {
			MatchData matchData = new MatchData {
				player1 = FirebaseManager.Instance.UserId,
				player2 = null,
				active = false,
				CreatedTime = DateTimeOffset.UtcNow,
				state = new GameState {
					turn = 0,
					playerOne = new PlayerState(),
					playerTwo = new PlayerState(),
				}
			};

			DatabaseReference matchReference = GamesReference.Push();
			matchData.id = matchReference.Key;
			await matchReference.SetRawJsonValueAsync(JsonUtility.ToJson(matchData));

			Debug.Log($"[MM] Created match with id: {matchData.id}");
			CurrentMatch = matchData;

			return matchData;
		}

		public static async void DeleteMatch(string id) {
			if (CurrentMatch != null && CurrentMatch.id == id)
				CurrentMatch = null;

			await GamesReference.Child(id).RemoveValueAsync();
		}

		/// <returns>Match data if successful, null otherwise.</returns>
		public static async Task<MatchData> TryJoinMatch(string matchId) {
			DatabaseReference matchReference = GamesReference.Child(matchId);
			DataSnapshot dataSnapshot;

			try {
				// Run a transaction to avoid a potential race condition.
				dataSnapshot = await matchReference.RunTransaction(data => {
					if (!data.HasChildren)
						return TransactionResult.Success(data);

					if ((bool)data.Child(nameof(MatchData.active)).Value) {
						// Someone else already joined.
						return TransactionResult.Abort();
					}

					// Write the data.
					data.Child(nameof(MatchData.player2)).Value = FirebaseManager.Instance.UserId;
					data.Child(nameof(MatchData.active)).Value = true;

					return TransactionResult.Success(data);
				});
			}
			catch (DatabaseException) {
				 // Transaction was aborted.
				 return null;
			}

			Debug.Assert(dataSnapshot != null, "dataSnapshot != null");

			if (!dataSnapshot.Exists) {
				// Match does not exist or was deleted.
				return null;
			}

			MatchData matchData = SnapshotToMatchData(dataSnapshot);

			Debug.Assert(matchData.active && matchData.player2 == FirebaseManager.Instance.UserId);

			CurrentMatch = matchData;
			return matchData;
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