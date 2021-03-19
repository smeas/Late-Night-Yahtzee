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

		private bool enabled;

		public GameHandler(string id) {
			GameId = id;
			Reference = MatchmakingManager.GamesReference.Child(id);
			playersReference = Reference.Child(nameof(GameInfo.players));
			statesReference = Reference.Child(nameof(GameInfo.states));
			turnReference = Reference.Child(nameof(GameInfo.turn));
		}

		public void Enable() {
			if (enabled) return;
			enabled = true;

			playersReference.ChildRemoved += OnPlayerRemoved;
			statesReference.ChildChanged += OnPlayerStateChanged;
			turnReference.ValueChanged += OnTurnChanged;
			// The only time children of this node (GameInfo) gets removed is if the whole game is removed.
			Reference.ChildRemoved += OnGameDeleted;
		}

		public void Disable() {
			if (!enabled) return;
			enabled = false;

			playersReference.ChildRemoved -= OnPlayerRemoved;
			statesReference.ChildChanged -= OnPlayerStateChanged;
			turnReference.ValueChanged -= OnTurnChanged;
			Reference.ChildRemoved -= OnGameDeleted;
		}

		private void OnPlayerRemoved(object sender, ChildChangedEventArgs e) {
			if (IsGameDeleted)
				return;

			int playerIndex = int.Parse(e.Snapshot.Key);
			string playerId = (string)e.Snapshot.Value;

			PlayerLeft?.Invoke(playerIndex, playerId);
		}

		private void OnPlayerStateChanged(object sender, ChildChangedEventArgs e) {
			if (IsGameDeleted)
				return;

			int playerIndex = int.Parse(e.Snapshot.Key); // TODO: Make safe
			PlayerState playerState = e.Snapshot.ToObject<PlayerState>();

			Debug.Assert(playerState != null, "playerState != null");

			PlayerStateChanged?.Invoke(playerIndex, playerState);
		}

		private void OnTurnChanged(object sender, ValueChangedEventArgs e) {
			if (IsGameDeleted)
				return;

			long? turn = e.Snapshot.Value as long?;
			if (turn == null) {
				Debug.Assert(false);
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