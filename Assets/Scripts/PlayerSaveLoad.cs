using System;
using UnityEngine;

public class PlayerSaveLoad : MonoBehaviour {
	[SerializeField] private GameObject[] players;
	[SerializeField] private bool loadOnStart = true;

	private void Start() {
		if (loadOnStart)
			LoadPlayers();
	}

	public async void SavePlayers() {
		SaveData saveData = SaveManager.Instance.SaveData;
		saveData.players = new PlayerInfo[players.Length];

		for (int i = 0; i < players.Length; i++) {
			saveData.players[i] = new PlayerInfo {
				name = players[i].name,
				position = players[i].transform.position
			};
		}

		await SaveManager.Instance.Save();
	}

	public async void LoadPlayers() {
		await SaveManager.Instance.Load();
		SaveData saveData = SaveManager.Instance.SaveData;
		if (saveData.players == null) return;

		int num = Mathf.Min(saveData.players.Length, players.Length);
		for (int i = 0; i < num; i++) {
			players[i].name = saveData.players[i].name;
			players[i].transform.position = saveData.players[i].position;

			NameTagManager.Instance.SetName(i, saveData.players[i].name);
		}
	}
}