using Managers;
using TMPro;
using UI.Modal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI {
	public class MainMenu : MonoBehaviour {
		[SerializeField] private SceneReference playScene;
		[SerializeField] private TextMeshProUGUI currentUserText;

		[Header("Screens")]
		[SerializeField] private GameObject loadingScreen;
		[SerializeField] private GameObject loginScreen;

		private MenuManager menuManager;
		private bool hasLoaded;

		private async void Start() {
			menuManager = GetComponent<MenuManager>();

			currentUserText.enabled = false;

			// Display a loading screen while setting up
			loadingScreen.SetActive(true);
			loginScreen.SetActive(false);
			menuManager.enabled = false;

			// Let firebase initialize before allowing the user to interact with the menus.
			await FirebaseManager.Instance.WaitForInitialization();

			if (FirebaseManager.Instance.IsSignedIn) {
				ShowMainMenu();
			}
			else {
				loadingScreen.SetActive(false);
				loginScreen.SetActive(true);
			}

			FirebaseManager.Instance.UsernameChanged += OnUsernameChanged;
			OnUsernameChanged(FirebaseManager.Instance.UserInfo?.username);
			currentUserText.enabled = true;
			hasLoaded = true;
		}

		private void OnDisable() {
			if (FirebaseManager.Instance == null)
				return;

			FirebaseManager.Instance.UsernameChanged -= OnUsernameChanged;
		}

		private void Update() {
			if (hasLoaded && menuManager.OpenMenuCount == 1 && Input.GetKeyDown(KeyCode.Escape)) {
				ModalManager.Instance.ShowYesNo("Are you sure you want to exit?", ok => {
					if (ok)
						Exit();
				});
			}
		}

		#region Event handlers

		private void OnUsernameChanged(string username) {
			string userText = null;

			if (FirebaseManager.Instance.IsSignedIn) {
				userText = "Signed in as: ";
				if (string.IsNullOrEmpty(username))
					userText += FirebaseManager.Instance.UserId;
				else
					userText += username;
			}

			currentUserText.text = userText;
		}

		public void OnChangeUsernamePressed() {
			ModalManager.Instance.ShowValue("Enter a new username", "Username", async username => {
				await FirebaseManager.Instance.UpdateUsername(username);
			});
		}

		#endregion

		public void ShowMainMenu() {
			loadingScreen.SetActive(false);
			loginScreen.SetActive(false);
			menuManager.enabled = true;
		}

		public void LoadGameScene() {
			SceneManager.LoadScene(playScene);
		}

		public void SignOut() {
			FirebaseManager.Instance.SignOut();
			loginScreen.SetActive(true);
			menuManager.enabled = false;
		}

		public void Exit() {
			Application.Quit();
		}
	}
}