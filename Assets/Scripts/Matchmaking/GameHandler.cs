using System;
using Extensions;
using Firebase.Database;
using UnityEngine;
using Yahtzee;

namespace Matchmaking {
	public class GameHandler {
		public string GameId { get; }
		public DatabaseReference Reference { get; }
		public bool IsGameDeleted { get; private set; }

		public event Action<int, string> PlayerLeft;
		public event Action<int, PlayerState> PlayerStateChanged;
		public event Action<int> TurnChanged;
		public event Action GameDeleted;

		private readonly DatabaseReference playersReference;
		private readonly DatabaseReference statesReference;
		private readonly DatabaseReference turnReference;
		private readonly DatabaseReference localPlayerReference;

		private bool enabled;

		public GameHandler(string id, int playerId) {
			GameId = id;
			Reference = MatchmakingManager.GamesReference.Child(id);
			playersReference = Reference.Child(nameof(GameInfo.players));
			statesReference = Reference.Child(nameof(GameInfo.states));
			turnReference = Reference.Child(nameof(GameInfo.turn));
			localPlayerReference = Reference.Child(nameof(GameInfo.players)).Child(playerId.ToString());
		}

		public void Enable() {
			if (enabled) return;
			enabled = true;

			// Remove the player from the game on disconnect.
			_ = localPlayerReference.OnDisconnect().RemoveValue();

			playersReference.ChildRemoved += OnPlayerRemoved;
			statesReference.ChildChanged += OnPlayerStateChanged;
			turnReference.ValueChanged += OnTurnChanged;
			// The only time children of this node (GameInfo) gets removed is if the whole game is removed.
			Reference.ChildRemoved += OnGameDeleted;
		}

		public void Disable() {
			if (!enabled) return;
			enabled = false;

			_ = localPlayerReference.OnDisconnect().Cancel();

			playersReference.ChildRemoved -= OnPlayerRemoved;
			statesReference.ChildChanged -= OnPlayerStateChanged;
			turnReference.ValueChanged -= OnTurnChanged;
			Reference.ChildRemoved -= OnGameDeleted;
		}

		private void OnPlayerRemoved(object sender, ChildChangedEventArgs e) {
			if (IsGameDeleted)
				return;

			if (!int.TryParse(e.Snapshot.Key, out int playerIndex)) {
				Debug.Assert(false);
				return;
			}

			string playerId = e.Snapshot.Value as string;
			if (playerId == null) {
				Debug.Assert(false);
				return;
			}

			PlayerLeft?.Invoke(playerIndex, playerId);
		}

		private void OnPlayerStateChanged(object sender, ChildChangedEventArgs e) {
			if (IsGameDeleted)
				return;

			if (!int.TryParse(e.Snapshot.Key, out int playerIndex)) {
				Debug.Assert(false);
				return;
			}

			PlayerState playerState = e.Snapshot.ToObject<PlayerState>();

			Debug.Assert(playerState != null, "playerState != null");

			PlayerStateChanged?.Invoke(playerIndex, playerState);
		}

		private void OnTurnChanged(object sender, ValueChangedEventArgs e) {
			if (IsGameDeleted)
				return;

			long? turn = e.Snapshot.Value as long?;
			if (turn == null) {
				Debug.Assert(false, "turn was null. Is Firebase being spooky?");
				return;
			}

			TurnChanged?.Invoke((int)turn);
		}

		private void OnGameDeleted(object sender, ChildChangedEventArgs e) {
			IsGameDeleted = true;

			GameDeleted?.Invoke();
		}
	}
}