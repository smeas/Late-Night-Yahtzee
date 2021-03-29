using System.Collections.Generic;
using Matchmaking;
using TMPro;
using UI.Modal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// TODO: Disable back button while joining, creating or starting a match.

namespace UI.Matchmaking {
	public class LobbyMenu : MonoBehaviour {
		[SerializeField] private Transform playerListRoot;
		[SerializeField] private Button startMatchButton;
		[SerializeField] private TextMeshProUGUI lobbyText;
		[SerializeField] private TextMeshProUGUI matchIdText;

		[Space]
		[SerializeField] private MenuManager menuManager;
		[SerializeField] private SceneReference gameScene;
		[SerializeField] private LobbyPlayerListItem playerListItemPrefab;

		private readonly List<LobbyPlayerListItem> playerListItems = new List<LobbyPlayerListItem>();

		private bool ownsMatch;
		private MatchHandler matchHandler;
		private MatchInfo match;
		private bool isGoingBack;

		private void OnEnable() {
			startMatchButton.interactable = false;
			matchIdText.text = "";

			// Clear the player list
			playerListItems.Clear();
			foreach (Transform item in playerListRoot)
				Destroy(item.gameObject);
		}

		private void OnDisable() {
			matchHandler?.Disable();
			ownsMatch = false;
			matchHandler = null;
			match = null;

			// Clear the player list
			playerListItems.Clear();
			foreach (Transform item in playerListRoot)
				Destroy(item.gameObject);
		}

		public void Show() {
			menuManager.PushMenu(gameObject);
		}

		public async void JoinMatch(string id) {
			ownsMatch = false;
			MatchInfo matchInfo = await MatchmakingManager.TryJoinMatch(id);

			if (matchInfo == null) {
				ModalManager.Instance.ShowInfo("Failed to join match", Back);
				return;
			}

			match = matchInfo;
			matchIdText.text = match.id;
			SetupMatchHandler();
		}

		public async void CreateMatch() {
			ownsMatch = true;

			match = await MatchmakingManager.CreateMatch();

			// Delete the match on disconnect
			await match.Reference.OnDisconnect().RemoveValue();

			matchIdText.text = match.id;

			SetupMatchHandler();
		}

		public async void Back() {
			if (isGoingBack) return;
			isGoingBack = true;

			if (matchHandler != null) {
				matchHandler.Disable();

				// Delete the match if we are the host
				if (ownsMatch)
					await matchHandler.Reference.RemoveValueAsync();
			}

			if (match != null) {
				await MatchmakingManager.LeaveMatch(match.id);
			}

			// Cleanup is handled by OnDisable
			menuManager.PopMenu();

			isGoingBack = false;
		}

		public async void StartMatch() {
			if (match == null) return;

			matchHandler.Disable();
			GameInfo gameInfo = await MatchmakingManager.StartMatch(match.id);
			EnterGame(gameInfo);
		}

		private void EnterGame(GameInfo gameInfo) {
			Debug.Log("[Lobby] Game starting");

			MatchmakingManager.ActiveGame = gameInfo;
			SceneManager.LoadScene(gameScene);
		}


		private void SetupMatchHandler() {
			// Ensure previous handler was disabled properly
			matchHandler?.Disable();

			matchHandler = new MatchHandler(match.id);

			matchHandler.PlayerJoined += MatchHandlerOnPlayerJoined;
			matchHandler.PlayerLeft += MatchHandlerOnPlayerLeft;
			matchHandler.PlayerChanged += MatchHandlerOnPlayerChanged;
			matchHandler.Start += MatchHandlerOnStart;
			matchHandler.MatchDeleted += MatchHandlerOnMatchDeleted;

			matchHandler.Enable();
		}


		#region Event handlers

		private void MatchHandlerOnStart(GameInfo gameInfo) {
			EnterGame(gameInfo);
		}

		private void MatchHandlerOnMatchDeleted() {
			Back();
		}

		private void MatchHandlerOnPlayerJoined(UserInfo playerInfo) {
			LobbyPlayerListItem listItem = Instantiate(playerListItemPrefab, playerListRoot);
			listItem.Initialize(playerInfo);

			playerListItems.Add(listItem);

			OnPlayerCountChanged();
		}

		private void MatchHandlerOnPlayerLeft(string playerId) {
			int index = playerListItems.FindIndex(item => item.PlayerId == playerId);
			if (index == -1)
				return;

			Destroy(playerListItems[index].gameObject);
			playerListItems.RemoveAt(index);

			OnPlayerCountChanged();
		}

		private void MatchHandlerOnPlayerChanged(int playerIndex, UserInfo playerInfo) {
			if (playerIndex >= playerListItems.Count) {
				Debug.Assert(false);
				return;
			}

			playerListItems[playerIndex].Initialize(playerInfo);
		}

		#endregion

		private void OnPlayerCountChanged() {
			startMatchButton.interactable = ownsMatch && playerListItems.Count >= 2;
			lobbyText.text = $"Lobby ({playerListItems.Count}/{MatchmakingManager.MaxPlayers})";
		}
	}
}