using Firebase.Database;
using Matchmaking;
using TMPro;
using UnityEngine;

namespace Yahtzee.UI {
	public class YahtzeeGame : MonoBehaviour {
		private const int MaxRolls = 3;

		// [SerializeField] private PlayerColumn playerOneColumn;
		// [SerializeField] private PlayerColumn playerTwoColumn;
		[SerializeField] private TextMeshProUGUI turnText;
		[SerializeField] private PlayerColumn[] playerColumns;
		[SerializeField] private DiceUI diceUI;

		private DatabaseReference matchReference;
		private DatabaseReference matchStateReference;
		private MatchData matchData;

		private PlayerIndex localPlayerIndex;
		private PlayerIndex otherPlayerIndex;
		private DiceSet dice = new DiceSet();
		private int[] currentDiceScores;
		private int rollCount;

		// State
		private PlayerIndex currentTurn;
		private PlayerState localPlayerState;
		private PlayerState otherPlayerState;

		private void Start() {
			Debug.Assert(playerColumns.Length == 2, "playerColumns.Length == 2");
			playerColumns[0].Initialize(this, PlayerIndex.PlayerOne);
			playerColumns[1].Initialize(this, PlayerIndex.PlayerTwo);
			diceUI.Dice = dice;

			matchData = MatchmakingManager.CurrentMatch;
			matchReference = MatchmakingManager.GamesReference.Child(matchData.id);
			matchStateReference = matchReference.Child(nameof(matchData.state));

			Debug.Assert(matchData.player1 != matchData.player2, "matchData.player1 != matchData.player2");

			if (matchData.player1 == FirebaseManager.Instance.UserId) {
				localPlayerIndex = PlayerIndex.PlayerOne;
				otherPlayerIndex = PlayerIndex.PlayerTwo;
			}
			else if (matchData.player2 == FirebaseManager.Instance.UserId) {
				localPlayerIndex = PlayerIndex.PlayerTwo;
				otherPlayerIndex = PlayerIndex.PlayerOne;
			}
			else {
				Debug.Assert(false);
			}

			// Grab the current state
			currentTurn = matchData.state.turn;
			if (localPlayerIndex == PlayerIndex.PlayerOne) {
				localPlayerState = matchData.state.playerOne;
				otherPlayerState = matchData.state.playerTwo;
			}
			else {
				localPlayerState = matchData.state.playerTwo;
				otherPlayerState = matchData.state.playerOne;
			}

			playerColumns[0].Data = matchData.state.playerOne;
			playerColumns[0].UpdateRepresentation();
			playerColumns[1].Data = matchData.state.playerTwo;
			playerColumns[1].UpdateRepresentation();

			SetupSync();

			turnText.text = currentTurn.ToString();
		}

		private void SetupSync() {
			matchStateReference.Child(nameof(matchData.state.turn)).ValueChanged += OnTurnChanged;
			matchStateReference
				.Child(localPlayerIndex == PlayerIndex.PlayerOne
					       ? nameof(matchData.state.playerTwo)
					       : nameof(matchData.state.playerOne)).ValueChanged += OnOtherPlayerStateChanged;
		}

		private void OnDisable() {
			matchStateReference.Child(nameof(matchData.state.turn)).ValueChanged -= OnTurnChanged;
			matchStateReference
				.Child(localPlayerIndex == PlayerIndex.PlayerOne
					       ? nameof(matchData.state.playerTwo)
					       : nameof(matchData.state.playerOne)).ValueChanged -= OnOtherPlayerStateChanged;
		}



		#region Event handlers

		private void OnTurnChanged(object sender, ValueChangedEventArgs e) {
			currentTurn = (PlayerIndex)(int)(long)e.Snapshot.GetValue(false); // FIXME: 99% gonna break

			print($"Turn changed: {currentTurn}");
			turnText.text = currentTurn.ToString();

			// TODO: Handle turn change
			if (currentTurn == localPlayerIndex) {
				rollCount = 0;
				// ...
			}
		}

		private void OnOtherPlayerStateChanged(object sender, ValueChangedEventArgs e) {
			string otherStateJson = e.Snapshot.GetRawJsonValue();
			otherPlayerState = JsonUtility.FromJson<PlayerState>(otherStateJson);

			playerColumns[(int)otherPlayerIndex].Data = otherPlayerState;
			playerColumns[(int)otherPlayerIndex].UpdateRepresentation();
		}

		public void OnScorePressed(PlayerIndex playerIndex, Category category) {
			print($"{playerIndex} pressed {category}");

			if (playerIndex != localPlayerIndex) return;
			if (currentTurn != localPlayerIndex) return;
			if (rollCount == 0) return;

			if (localPlayerState.CanChoose(category)) {
				if (currentDiceScores[(int)category] != 0) {
					ScoreCategory(category);
				}
				else {
					ScratchCategory(category);
				}

				EndTurn();
			}
		}

		public void OnRollPressed() {
			if (currentTurn != localPlayerIndex) return;
			if (rollCount < MaxRolls)
				RollDice();
		}

		#endregion

		private void ScoreCategory(Category category) {
			print($"Score {category}");
			localPlayerState[category] = currentDiceScores[(int)category];
			playerColumns[(int)localPlayerIndex].UpdateRepresentation();
		}

		private void ScratchCategory(Category category) {
			print($"Scratch {category}");
			localPlayerState.Scratch(category);
			playerColumns[(int)localPlayerIndex].UpdateRepresentation();
		}

		private void RollDice() {
			print("Roll");
			dice.Roll();
			currentDiceScores = dice.CalculateScores();
			diceUI.UpdateRepresentation();
			rollCount++;
		}

		private async void EndTurn() {
			Debug.Assert(currentTurn == localPlayerIndex);
			print("End turn");

			// Set the turn locally first as it is used to determine whether the player can perform actions.
			currentTurn = otherPlayerIndex;

			// Update the local player state on the database.
			string localStateJson = JsonUtility.ToJson(localPlayerState);
			await matchStateReference
				.Child(localPlayerIndex == PlayerIndex.PlayerOne
					       ? nameof(GameState.playerOne)
					       : nameof(GameState.playerTwo)).SetRawJsonValueAsync(localStateJson);

			// Update the current turn on the database.
			await matchStateReference.Child(nameof(GameState.turn)).SetValueAsync((int)currentTurn); // TODO: What happens if I pass an enum here?
		}

	}
}