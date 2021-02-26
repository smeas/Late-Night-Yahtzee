using Firebase.Database;
using Matchmaking;
using TMPro;
using UI.Modal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Yahtzee.UI {
	public class YahtzeeGame : MonoBehaviour {
		private const int MaxRolls = 3;

		[SerializeField] private TextMeshProUGUI turnText;
		[SerializeField] private PlayerColumn[] playerColumns;
		[SerializeField] private DiceUI diceUI;
		[SerializeField] private SceneReference menuScene;

		private DatabaseReference matchReference;
		private DatabaseReference matchStateReference;
		private MatchData matchData;

		private PlayerIndex localPlayerIndex;
		private PlayerIndex otherPlayerIndex;
		private DiceSet diceSet = new DiceSet();
		private int[] currentDiceScores;
		private int rollCount;

		// State
		private PlayerIndex currentTurn;
		private PlayerState localPlayerState;
		private PlayerState otherPlayerState;

		private bool gameDeleted;
		private bool gameOver;

		private void Start() {
			Debug.Assert(playerColumns.Length == 2, "playerColumns.Length == 2");
			playerColumns[0].Initialize(this, PlayerIndex.PlayerOne);
			playerColumns[1].Initialize(this, PlayerIndex.PlayerTwo);
			diceUI.Initialize(this, diceSet);

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
			if (localPlayerIndex == PlayerIndex.PlayerTwo) {
				// TODO: Maybe do something cleaner to signal this event?
				matchReference.OnDisconnect().RemoveValue();
			}

			matchStateReference.Child(nameof(matchData.state.turn)).ValueChanged += OnTurnChanged;

			// Listen to changes in the other player's state
			matchStateReference
				.Child(localPlayerIndex == PlayerIndex.PlayerOne
					       ? nameof(matchData.state.playerTwo)
					       : nameof(matchData.state.playerOne)).ValueChanged += OnOtherPlayerStateChanged;

			// We never remove individual child nodes from the match data. So if a child gets removed, it means that the
			// whole match is getting removed. This usually means that the opponent quit the game.
			matchReference.ChildRemoved += OnMatchDeleted;
		}

		private void OnDisable() {
			matchStateReference.Child(nameof(matchData.state.turn)).ValueChanged -= OnTurnChanged;
			matchStateReference
				.Child(localPlayerIndex == PlayerIndex.PlayerOne
					       ? nameof(matchData.state.playerTwo)
					       : nameof(matchData.state.playerOne)).ValueChanged -= OnOtherPlayerStateChanged;

			matchReference.ChildRemoved -= OnMatchDeleted;
		}

		private void Update() {
			// Allow exiting to menu
			if (!ModalManager.Instance.IsShowing && Input.GetKeyDown(KeyCode.Escape)) {
				ModalManager.Instance.ShowYesNo("Are you sure you want to exit?", ok => {
					if (ok) {
						ExitToMenu();
					}
				});
			}
		}


		#region Event handlers

		private void OnTurnChanged(object sender, ValueChangedEventArgs e) {
			object value = e.Snapshot.GetValue(false);
			if (value == null)
				return;

			currentTurn = (PlayerIndex)(int)(long)e.Snapshot.GetValue(false);

			print($"[Game] Turn changed: {currentTurn}");
			turnText.text = currentTurn.ToString();

			if (currentTurn == localPlayerIndex && localPlayerState.IsFinished ||
				currentTurn == otherPlayerIndex && otherPlayerState.IsFinished) {
				GameOver();
				return;
			}

			if (currentTurn == localPlayerIndex) {
				diceUI.CanRoll = true;
			}
		}

		private void OnOtherPlayerStateChanged(object sender, ValueChangedEventArgs e) {
			string otherStateJson = e.Snapshot.GetRawJsonValue();
			if (otherStateJson == null)
				return;

			otherPlayerState = JsonUtility.FromJson<PlayerState>(otherStateJson);
			otherPlayerState.UpdateSums();

			playerColumns[(int)otherPlayerIndex].Data = otherPlayerState;
			playerColumns[(int)otherPlayerIndex].UpdateRepresentation();
		}

		private void OnMatchDeleted(object sender, ChildChangedEventArgs e) {
			if (gameDeleted) return;
			gameDeleted = true;

			print("[Game] The match has been deleted!");

			if (!gameOver)
				ModalManager.Instance.ShowInfo("Your opponent has forfeited the game!", ExitToMenu);
		}

		public void OnScorePressed(PlayerIndex playerIndex, Category category) {
			print($"[Game] {playerIndex} pressed {category}");

			if (playerIndex != localPlayerIndex) return;
			if (currentTurn != localPlayerIndex) return;
			if (rollCount == 0) return;

			if (localPlayerState.CanChoose(category)) {
				if (category == Category.Bonus && localPlayerState.HasGottenBonus)
					return;

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
			print($"[Game] Score {category}");
			localPlayerState[category] = currentDiceScores[(int)category];
			localPlayerState.UpdateSums();
			playerColumns[(int)localPlayerIndex].UpdateRepresentation();
		}

		private void ScratchCategory(Category category) {
			print($"[Game] Scratch {category}");
			localPlayerState.Scratch(category);
			localPlayerState.UpdateSums();
			playerColumns[(int)localPlayerIndex].UpdateRepresentation();
		}

		private void RollDice() {
			print("[Game] Roll");
			diceSet.Roll();
			currentDiceScores = diceSet.CalculateScores();
			rollCount++;

			if (rollCount == 1) {
				// After first roll
				diceUI.BlankDice = false;
				diceUI.CanLock = true;
			}
			else if (rollCount == MaxRolls) {
				// After last roll
				diceUI.CanRoll = false;
			}

			diceUI.UpdateRepresentation();
		}

		private async void EndTurn() {
			Debug.Assert(currentTurn == localPlayerIndex);
			print("[Game] End turn");

			rollCount = 0;
			diceSet.UnlockAll();
			diceUI.BlankDice = true;
			diceUI.CanLock = false;
			diceUI.CanRoll = false;
			diceUI.UpdateRepresentation();

			// Set the turn locally first as it is used to determine whether the player can perform actions.
			currentTurn = otherPlayerIndex;

			// Update the local player state on the database.
			string localStateJson = JsonUtility.ToJson(localPlayerState);
			await matchStateReference
				.Child(localPlayerIndex == PlayerIndex.PlayerOne
					       ? nameof(GameState.playerOne)
					       : nameof(GameState.playerTwo)).SetRawJsonValueAsync(localStateJson);

			// Update the current turn on the database.
			await matchStateReference.Child(nameof(GameState.turn)).SetValueAsync((int)currentTurn);
		}

		private void GameOver() {
			Debug.Log("[Game] Game over!");

			Debug.Assert(
				currentTurn == localPlayerIndex ? otherPlayerState.IsFinished : localPlayerState.IsFinished,
				"Other player also finished");

			gameOver = true;

			if (localPlayerState.TotalSum == otherPlayerState.TotalSum) {
				ModalManager.Instance.ShowInfo("Game over.\nIt's a tie!", ExitToMenu);
			}
			else {
				PlayerIndex winner = localPlayerState.TotalSum > otherPlayerState.TotalSum
					? localPlayerIndex
					: otherPlayerIndex;

				ModalManager.Instance.ShowInfo($"Game over.\n{winner} wins!", ExitToMenu);
			}
		}

		private async void ExitToMenu() {
			if (!gameDeleted) {
				// TODO: Maybe do something cleaner to signal this event?
				gameDeleted = true;
				await matchReference.RemoveValueAsync();
			}

			SceneManager.LoadScene(menuScene);
		}
	}
}