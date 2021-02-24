using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	[SerializeField] private TMP_InputField[] nameFields;
	[SerializeField] private SceneReference playScene;
	[SerializeField] private TextMeshProUGUI currentUserText;

	[Header("Screens")]
	[SerializeField] private GameObject loadingScreen;
	[SerializeField] private GameObject loginScreen;
	[SerializeField] private GameObject menuScreen;

	private async void Start() {
		loadingScreen.SetActive(true);
		loginScreen.SetActive(false);
		menuScreen.SetActive(false);

		// Let firebase initialize before allowing the user to interact with the menus.
		await FirebaseManager.Instance.WaitForInitialization();

		if (FirebaseManager.Instance.IsSignedIn) {
			await ShowMainMenuInternal();
		}
		else {
			loadingScreen.SetActive(false);
			loginScreen.SetActive(true);
		}

		OnAuthStateChanged(null, null);
	}

	private async void OnEnable() {
		await FirebaseManager.Instance.WaitForInitialization();
		FirebaseManager.Instance.Auth.StateChanged += OnAuthStateChanged;
	}

	private void OnDisable() {
		FirebaseManager.Instance.Auth.StateChanged -= OnAuthStateChanged;
	}

	private void OnAuthStateChanged(object sender, EventArgs e) {
		currentUserText.text = FirebaseManager.Instance.User?.UserId;
	}

	public async void ShowMainMenu() {
		await ShowMainMenuInternal();
	}

	public async void Save() {
		SaveData saveData = SaveManager.Instance.SaveData;

		if (nameFields.Length > saveData.players.Length)
			Array.Resize(ref saveData.players, nameFields.Length);

		for (int i = 0; i < nameFields.Length; i++) {
			saveData.players[i] ??= new PlayerInfo();
			saveData.players[i].name = nameFields[i].text;
		}

		await SaveManager.Instance.Save();
	}

	public void Play() {
		Save();
		SceneManager.LoadScene(playScene);
	}

	public void LoadGameScene() {
		SceneManager.LoadScene(playScene);
	}

	public void SignOut() {
		FirebaseManager.Instance.SignOut();
		loginScreen.SetActive(true);
		menuScreen.SetActive(false);
	}

	private async Task ShowMainMenuInternal() {
		await Load();

		loadingScreen.SetActive(false);
		loginScreen.SetActive(false);
		menuScreen.SetActive(true);
	}

	private async Task Load() {
		await SaveManager.Instance.Load();
		SaveData saveData = SaveManager.Instance.SaveData;

		// Clear the fields.
		foreach (TMP_InputField field in nameFields)
			field.text = "";

		// Load names from the save data into the fields.
		int num = Mathf.Min(saveData.players.Length, nameFields.Length);
		for (int i = 0; i < num; i++) {
			if (string.IsNullOrEmpty(nameFields[i].text))
				nameFields[i].text = saveData.players[i].name;
		}
	}
}