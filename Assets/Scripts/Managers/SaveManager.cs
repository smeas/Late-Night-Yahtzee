using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class SaveManager : SingletonBehaviour<SaveManager> {
	private const string SaveFile = "save.json";
	private const string RemoteSaveUri = "http://localhost:8080/";

	private HttpClient httpClient;

	public SaveData SaveData { get; private set; } = new SaveData();

	protected override void Awake() {
		base.Awake();

		httpClient = new HttpClient(new HttpClientHandler {UseProxy = false}) {
			Timeout = TimeSpan.FromSeconds(5),
		};
	}

	protected override void OnDestroy() {
		base.OnDestroy();

		httpClient.CancelPendingRequests();
	}

	public async Task Save() {
		string data = JsonUtility.ToJson(SaveData);
		WriteFile(SaveFile, data);

		try {
			await FirebaseManager.Instance.SaveUserData(data);
			print("Saved data on firebase");
		}
		catch (Exception ex) {
			Debug.LogError($"Failed to save data to firebase: {ex}");
		}
	}

	public async Task Load() {
		string data = null;

		try {
			data = await FirebaseManager.Instance.LoadUserData();
		}
		catch (Exception ex) {
			Debug.LogError($"Failed to load data from firebase.\n{ex}");

			// // Fallback to local save file.
			// data = ReadFile(SaveFile);
		}

		if (data != null)
			SaveData = JsonUtility.FromJson<SaveData>(data);
		else
			SaveData = new SaveData();
	}


	// Local file system

	private static void WriteFile(string path, string data) {
		path = Path.Combine(Application.persistentDataPath, path);

		try {
			File.WriteAllText(path, data);
		}
		catch (IOException e) {
			Debug.LogError($"Failed to write file: {e}");
		}
	}

	private static string ReadFile(string path) {
		path = Path.Combine(Application.persistentDataPath, path);

		if (!File.Exists(path))
			return null;

		try {
			return File.ReadAllText(path);
		}
		catch (IOException e) {
			Debug.LogError($"Failed to read file: {e}");
			return null;
		}
	}

	// JSON Slave

	private async Task SaveRemote(string path, string data) {
		try {
			await httpClient.PutAsync(RemoteSaveUri + path, new StringContent(data));
		}
		catch (HttpRequestException e) {
			Debug.LogError($"Failed to upload save data to remote: {e}");
		}
	}

	private async Task<string> LoadRemote(string path) {
		try {
			return await httpClient.GetStringAsync(RemoteSaveUri + path);
		}
		catch (HttpRequestException e) {
			Debug.LogError($"Failed to fetch save data from remote: {e}");
		}

		return null;
	}
}

[Serializable]
public class SaveData {
	public PlayerInfo[] players = Array.Empty<PlayerInfo>();
}

[Serializable]
public class PlayerInfo {
	public string name;
	public Vector2 position;
}