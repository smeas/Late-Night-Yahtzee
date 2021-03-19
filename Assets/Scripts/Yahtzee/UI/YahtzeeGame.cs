using System;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
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
		[SerializeField] private DiceUI3D diceUI;
		[SerializeField] private Sheet sheet;
		[SerializeField] private SceneReference menuScene;

		private DatabaseReference gameReference;
		private DatabaseReference myPlayerStateReference;
		private DatabaseReference turnReference;
		private GameHandler gameHandler;
		private GameInfo gameInfo;

		private readonly DiceSet diceSet = new DiceSet();
		private int[] currentDiceScores;
		private int rollCount;

		private int myPlayerIndex;
		private string[] playerIds;
		private string[] playerNames;
		private PlayerState[] playerStates;

		// State
		private int currentTurn;

		private bool gameDeleted;
		private bool gameOver;
		private bool isRolling;

		// Convenience properties
		private int PlayerCount => playerIds.Length;
		private bool IsMyTurn => currentTurn == myPlayerIndex;
		private PlayerState MyPlayerState => playerStates[myPlayerIndex];

		private async void Start() {
			if (MatchmakingManager.ActiveGame == null) {
				Debug.LogError("[Game] No active game");
				return;
			}

			// Make sure we have up to date game info
			gameInfo = await MatchmakingManager.GetGame(MatchmakingManager.ActiveGame.id);
			playerIds = gameInfo.players;
			currentTurn = gameInfo.turn;
			playerStates = gameInfo.states;
			myPlayerIndex = Array.IndexOf(playerIds, FirebaseManager.Instance.UserId);

			Debug.Assert(myPlayerIndex != -1, "localPlayerIndex != -1");

			await FetchUsernames();

			gameReference = gameInfo.Reference;
			myPlayerStateReference = gameReference.Child(nameof(GameInfo.states)).Child(myPlayerIndex.ToString());
			turnReference = gameReference.Child(nameof(GameInfo.turn));

			// Initialize game UI
			sheet.Initialize(this, PlayerCount, playerNames, playerStates);
			diceUI.Initialize(this, diceSet);
			diceUI.BlankDice = true;
			diceUI.CanLock = false;
			diceUI.CanRoll = IsMyTurn;
			diceUI.RollCompleted += OnRollCompleted;

			turnText.text = playerNames[currentTurn];

			ShakeDetector.Instance.Shake += OnRollPressed;

			SetupGameHandler();
		}

		private void OnDisable() {
			// Clean up handler
			gameHandler?.Disable();
		}

		private async Task FetchUsernames() {
			playerNames = new string[playerIds.Length];

			for (int i = 0; i < playerIds.Length; i++) {
				playerNames[i] = await FirebaseManager.Instance.GetUsername(playerIds[i]);
			}
		}

		private void SetupGameHandler() {
			gameHandler = new GameHandler(gameInfo.id);

			gameHandler.TurnChanged += OnTurnChanged;
			gameHandler.PlayerStateChanged += OnPlayerStateChanged;
			gameHandler.PlayerLeft += OnPlayerLeft;
			gameHandler.GameDeleted += OnGameDeleted;

			gameHandler.Enable();
		}

		// private async void Start() {
		// 	sheet.Initialize(this, 2);
		// 	diceUI.Initialize(this, diceSet);
		//
		// 	// Grab match data and DB references
		// 	matchInfo = MatchmakingManager.CurrentMatch;
		// 	matchReference = MatchmakingManager.GamesReference.Child(matchInfo.id);
		// 	matchStateReference = matchReference.Child(nameof(matchInfo.state));
		//
		// 	Debug.Assert(matchInfo.player1 != matchInfo.player2, "matchData.player1 != matchData.player2");
		//
		// 	// Setup player indices
		// 	if (matchInfo.player1 == FirebaseManager.Instance.UserId) {
		// 		localPlayerIndex = PlayerIndex.PlayerOne;
		// 		otherPlayerIndex = PlayerIndex.PlayerTwo;
		// 	}
		// 	else if (matchInfo.player2 == FirebaseManager.Instance.UserId) {
		// 		localPlayerIndex = PlayerIndex.PlayerTwo;
		// 		otherPlayerIndex = PlayerIndex.PlayerOne;
		// 	}
		// 	else {
		// 		Debug.Assert(false, "Current user is part of the match");
		// 	}
		//
		// 	// Grab the current state
		// 	currentTurn = matchInfo.state.turn;
		// 	if (localPlayerIndex == PlayerIndex.PlayerOne) {
		// 		localPlayerState = matchInfo.state.playerOne;
		// 		otherPlayerState = matchInfo.state.playerTwo;
		// 	}
		// 	else {
		// 		localPlayerState = matchInfo.state.playerTwo;
		// 		otherPlayerState = matchInfo.state.playerOne;
		// 	}
		//
		// 	// Get user infos
		// 	playerOneInfo = await FirebaseManager.Instance.GetUserInfo(matchInfo.player1);
		// 	playerTwoInfo = await FirebaseManager.Instance.GetUserInfo(matchInfo.player2);
		//
		// 	sheet.PlayerColumns[0].Name = playerOneInfo.username;
		// 	sheet.PlayerColumns[1].Name = playerTwoInfo.username;
		//
		// 	// Update the columns with the data
		// 	sheet.PlayerColumns[0].Data = matchInfo.state.playerOne;
		// 	sheet.PlayerColumns[0].UpdateRepresentation();
		// 	sheet.PlayerColumns[1].Data = matchInfo.state.playerTwo;
		// 	sheet.PlayerColumns[1].UpdateRepresentation();
		//
		// 	turnText.text = currentTurn == PlayerIndex.PlayerOne ? playerOneInfo.username : playerTwoInfo.username;
		//
		// 	diceUI.BlankDice = true;
		// 	diceUI.CanLock = false;
		// 	diceUI.CanRoll = localPlayerIndex == PlayerIndex.PlayerOne;
		// 	diceUI.RollCompleted += OnRollCompleted;
		//
		// 	ShakeDetector.Instance.Shake += OnRollPressed;
		//
		// 	SetupSync();
		// }

		// private void SetupSync() {
		// 	if (localPlayerIndex == PlayerIndex.PlayerTwo) {
		// 		// TODO: Maybe do something cleaner to signal this event?
		// 		matchReference.OnDisconnect().RemoveValue();
		// 	}
		//
		// 	matchStateReference.Child(nameof(matchInfo.state.turn)).ValueChanged += OnTurnChanged;
		//
		// 	// Listen to changes in the other player's state
		// 	matchStateReference
		// 		.Child(localPlayerIndex == PlayerIndex.PlayerOne
		// 			       ? nameof(matchInfo.state.playerTwo)
		// 			       : nameof(matchInfo.state.playerOne)).ValueChanged += OnOtherPlayerStateChanged;
		//
		// 	// We never remove individual child nodes from the match data. So if a child gets removed, it means that the
		// 	// whole match is getting removed. This usually means that the opponent quit the game.
		// 	matchReference.ChildRemoved += OnMatchDeleted;
		// }
		//
		// private void OnDisable() {
		// 	matchStateReference.Child(nameof(matchInfo.state.turn)).ValueChanged -= OnTurnChanged;
		// 	matchStateReference
		// 		.Child(localPlayerIndex == PlayerIndex.PlayerOne
		// 			       ? nameof(matchInfo.state.playerTwo)
		// 			       : nameof(matchInfo.state.playerOne)).ValueChanged -= OnOtherPlayerStateChanged;
		//
		// 	matchReference.ChildRemoved -= OnMatchDeleted;
		//
		// 	if (ShakeDetector.Instance != null)
		// 		ShakeDetector.Instance.Shake -= OnRollPressed;
		// }
		//
		// private void Update() {
		// 	// Allow exiting to menu
		// 	if (!ModalManager.Instance.IsShowing && Input.GetKeyDown(KeyCode.Escape)) {
		// 		ModalManager.Instance.ShowYesNo("Are you sure you want to exit?", ok => {
		// 			if (ok) {
		// 				ExitToMenu();
		// 			}
		// 		});
		// 	}
		// }


		#region Event handlers

		private void OnTurnChanged(int newTurn) {
			currentTurn = newTurn;

			print($"[Game] Turn changed: {currentTurn}");
			turnText.text = playerNames[currentTurn];

			// Check if game is finished
			if (playerStates[currentTurn].IsFinished) {
				GameOver();
				return;
			}

			if (IsMyTurn) {
				diceUI.CanRoll = true;
			}
		}

		private void OnPlayerStateChanged(int playerIndex, PlayerState state) {
			if (playerIndex == myPlayerIndex) // ignore our own state change
				return;

			playerStates[playerIndex] = state;
			playerStates[playerIndex].UpdateSums();

			sheet.PlayerColumns[playerIndex].Data = state;
			sheet.PlayerColumns[playerIndex].UpdateRepresentation();
		}

		private void OnGameDeleted() {
			if (gameDeleted || gameOver) return;
			gameDeleted = true;

			print("[Game] The game has been deleted!");

			if (!gameOver)
				ModalManager.Instance.ShowInfo("yYour opponent(s) have forfeited the game!", ExitToMenu);
		}

		private void OnPlayerLeft(int playerIndex, string playerId) {
			// TODO: Let all but two players leave without stopping the game.
			print($"[Game] Player {playerIndex} '{playerNames[playerIndex]}' left");

			ExitToMenu();
		}

		public void OnCategoryPressed(Category category) {
			print($"[Game] Pressed {category}");

			if (!IsMyTurn) return;
			if (rollCount == 0) return;

			if (MyPlayerState.CanChoose(category)) {
				if (category == Category.Bonus && MyPlayerState.HasGottenBonus)
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
			if (IsMyTurn && rollCount < MaxRolls)
				RollDice();
		}

		private void OnRollCompleted() {
			print("[Game] Roll complete");

			isRolling = false;

			currentDiceScores = diceSet.CalculateScores();
			rollCount++;

			// Show available move hints
			sheet.PlayerColumns[myPlayerIndex].CurrentRollScores = currentDiceScores;
			sheet.PlayerColumns[myPlayerIndex].ShowHints = true;
			sheet.PlayerColumns[myPlayerIndex].UpdateRepresentation();

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

		#endregion

		private void ScoreCategory(Category category) {
			print($"[Game] Score {category}");

			MyPlayerState[category] = currentDiceScores[(int)category];
			MyPlayerState.UpdateSums();

			sheet.PlayerColumns[myPlayerIndex].UpdateRepresentation();
		}

		private void ScratchCategory(Category category) {
			print($"[Game] Scratch {category}");

			MyPlayerState.Scratch(category);
			MyPlayerState.UpdateSums();

			sheet.PlayerColumns[myPlayerIndex].UpdateRepresentation();
		}

		private void RollDice() {
			if (isRolling)
				return;

			print("[Game] Roll");
			diceUI.RollDice();
			isRolling = true;
		}

		private async void EndTurn() {
			Debug.Assert(IsMyTurn);
			print("[Game] End turn");

			rollCount = 0;
			diceSet.UnlockAll();
			diceUI.BlankDice = true;
			diceUI.CanLock = false;
			diceUI.CanRoll = false;
			diceUI.UpdateRepresentation();

			sheet.PlayerColumns[myPlayerIndex].ShowHints = false;
			sheet.PlayerColumns[myPlayerIndex].UpdateRepresentation();

			// Set the turn locally first as it is used to determine whether the player can perform actions.
			currentTurn = (currentTurn + 1) % PlayerCount;

			// Update the local player state on the database.
			await myPlayerStateReference.SetValueFromObjectAsJson(MyPlayerState);

			// Update the current turn on the database.
			await turnReference.SetValueAsync(currentTurn);
		}

		private void GameOver() {
			Debug.Log("[Game] Game over!");

			Debug.Assert(playerStates.All(state => state.IsFinished), "All players finished on game over");

			gameOver = true;

			// Calculate winners
			(int playerIndex, int score)[] winners = playerStates
				.Select((state, playerIndex) => (playerIndex, state.TotalSum))
				.OrderByDescending(x => x.TotalSum)
				.GroupBy(x => x.TotalSum)
				.First()
				.ToArray();

			if (winners.Length == 1) {
				string winner = playerNames[winners[0].playerIndex];
				ModalManager.Instance.ShowInfo($"Game over.\n{winner} wins!", ExitToMenu);
			}
			else {
				string winnersString = string.Join(", ", winners.Select(x => playerNames[x.playerIndex]));
				ModalManager.Instance.ShowInfo($"It's a tie!\nWinners: {winnersString}.", ExitToMenu);
			}
		}

		private async void ExitToMenu() {
			if (!gameDeleted) {
				// TODO: Maybe do something cleaner to signal this event?
				gameDeleted = true;
				await gameReference.RemoveValueAsync();
			}

			SceneManager.LoadScene(menuScene);
		}
	}
}