using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
	[SerializeField] private TMP_InputField[] nameFields;
	[SerializeField] private SceneReference playScene;

	private async void Start() {
		await SaveManager.Instance.Load();
		SaveData saveData = SaveManager.Instance.SaveData;

		int num = Mathf.Min(saveData.players.Length, nameFields.Length);
		for (int i = 0; i < num; i++) {
			if (string.IsNullOrEmpty(nameFields[i].text))
				nameFields[i].text = saveData.players[i].name;
		}
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
}