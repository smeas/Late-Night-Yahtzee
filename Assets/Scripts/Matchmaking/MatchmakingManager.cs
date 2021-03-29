using System;
using System.Threading.Tasks;
using Extensions;
using Firebase.Database;
using Managers;
using UnityEngine;
using Utilities;
using Yahtzee;

namespace Matchmaking {
	public static class MatchmakingManager {
		public const int MaxPlayers = 4;

		public static DatabaseReference MatchesReference { get; } =
			FirebaseManager.Instance.RootReference.Child("matches");

		public static DatabaseReference GamesReference { get; } =
			FirebaseManager.Instance.RootReference.Child("games");

		/// <summary>
		/// The active game. Shared across the menu and game scenes.
		/// </summary>
		public static GameInfo ActiveGame { get; set; }


		public static async Task<MatchInfo> CreateMatch() {
			// Create a unique id for the new match
			DatabaseReference matchReference = MatchesReference.Push();

			MatchInfo matchInfo = new MatchInfo {
				id = matchReference.Key,
				CreatedTime = DateTimeOffset.UtcNow,
				isStarting = false,
				started = false,
				playerCount = 1,
				players = new[] {FirebaseManager.Instance.UserId},
			};

			await matchReference.SetValueFromObjectAsJson(matchInfo);
			Debug.Log($"[MM] Created match with id: {matchInfo.id}");
			return matchInfo;
		}

		public static async Task<MatchInfo> TryJoinMatch(string id) {
			DatabaseReference matchReference = MatchesReference.Child(id);
			DataSnapshot snap;
			bool aborted = false;

			try {
				snap = await matchReference.RunTransaction(data => {
					if (!data.HasChildren) // Cheaper way of checking for null data
						return TransactionResult.Success(data);

					// Check if we can join
					int playerCount = (int)(long)data.Child(nameof(MatchInfo.playerCount)).Value;
					bool isStarting = (bool)data.Child(nameof(MatchInfo.isStarting)).Value;
					if (playerCount >= MaxPlayers || isStarting) {
						aborted = true;
						return TransactionResult.Abort();
					}

					// Add our user id to the player list and update the player count
					data.Child($"{nameof(MatchInfo.players)}/{playerCount}").Value = FirebaseManager.Instance.UserId;
					data.Child(nameof(MatchInfo.playerCount)).Value = playerCount + 1;

					return TransactionResult.Success(data);
				});
			}
			catch (DatabaseException) {
				// Transaction was aborted, failed to join
				return null;
			}

			if (aborted || snap == null || !snap.HasChildren)
				return null;

			MatchInfo matchInfo = snap.ToObjectWithId<MatchInfo>();
			Debug.Log($"[MM] Joined match with id: {id}");
			return matchInfo;
		}

		public static async Task<GameInfo> StartMatch(string id) {
			DatabaseReference matchReference = MatchesReference.Child(id);
			MatchInfo match = await GetMatch(id);

			if (match == null) {
				Debug.LogWarning("[MM] Match not found.");
				return null; // Match not found
			}

			if (match.playerCount < 2) {
				Debug.LogWarning("[MM] Not enough players to start match.");
				return null; // Not enough players to start
			}

			if (match.isStarting || match.started) {
				Debug.LogWarning("[MM] Match already started/starting.");
				return null; // Already started/starting
			}

			// Prevent new players from joining
			await matchReference.Child(nameof(MatchInfo.isStarting)).SetValueAsync(true);

			// Fetch the match again in case anything has changed
			match = await GetMatch(id);

			// TODO: Check player count again. It could have change since the last fetch.
			// FIXME(RC): What happens if a player leaves after this point but before the game actually starts? The player
			// leaving would go unhandled and the player would still be considered as part of the game.

			// Construct a new game using the same id as the match
			DatabaseReference gameReference = GamesReference.Child(id);
			GameInfo gameInfo = new GameInfo {
				id = id,
				players = match.players,
				playerCount = match.playerCount,
				states = Util.CreateInitializedArray<PlayerState>(match.playerCount),
				turn = 0,
			};

			await gameReference.SetValueFromObjectAsJson(gameInfo);

			// Notify players that the game has been created and is starting and remove the match
			await matchReference.Child(nameof(MatchInfo.started)).SetValueAsync(true);
			await matchReference.RemoveValueAsync();

			return gameInfo;
		}

		public static async Task<MatchInfo> GetMatch(string id) {
			DataSnapshot snapshot = await MatchesReference.Child(id).GetValueAsync();
			return snapshot.ToObjectWithId<MatchInfo>();
		}

		public static async Task<GameInfo> GetGame(string id) {
			DataSnapshot snapshot = await GamesReference.Child(id).GetValueAsync();
			return snapshot.ToObjectWithId<GameInfo>();
		}
	}
}