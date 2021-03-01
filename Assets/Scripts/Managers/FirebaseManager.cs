using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class FirebaseManager : SingletonBehaviour<FirebaseManager> {
	public FirebaseAuth Auth { get; private set; }
	public FirebaseDatabase Database { get; private set; }

	public DatabaseReference RootReference => Database.RootReference;
	public DatabaseReference UserReference => Database.RootReference.Child("users/" + User.UserId);

	public string UserId => User.UserId;
	public FirebaseUser User => Auth.CurrentUser;
	public UserInfo UserInfo { get; private set; }

	public bool IsSignedIn => User != null;
	public bool Initialized { get; private set; }

	private Task initTask;


	public event Action<string> UsernameChanged;


	protected override void Awake() {
		base.Awake();

		DontDestroyOnLoad(this);

		initTask = Initialize();
	}

	private async Task Initialize() {
		await FirebaseApp.CheckAndFixDependenciesAsync();
		Debug.Log("[FB] Firebase initialized");

		Database = FirebaseDatabase.DefaultInstance;
		Auth = FirebaseAuth.DefaultInstance;

		// Fetch existing user info
		if (IsSignedIn) {
			await FetchUserInfo();
			UsernameChanged?.Invoke(UserInfo.username);
		}

		Initialized = true;
		initTask = null;
	}

	public async Task WaitForInitialization() {
		if (!Initialized)
			await initTask;
	}

	#region Auth

	public async Task RegisterUser(string email, string password) {
		if (!Initialized)
			await initTask;

		await Auth.CreateUserWithEmailAndPasswordAsync(email, password);

		UserInfo = new UserInfo {id = UserId};
		await PushUserInfo();
		UsernameChanged?.Invoke(null);
	}

	public async Task SignInUser(string email, string password) {
		if (!Initialized)
			await initTask;

		await Auth.SignInWithEmailAndPasswordAsync(email, password);

		await FetchUserInfo();
		UsernameChanged?.Invoke(UserInfo.username);
	}

	public async Task SignInAnonymously() {
		if (!Initialized)
			await initTask;

		await Auth.SignInAnonymouslyAsync();

		UserInfo = new UserInfo {id = UserId};
		await PushUserInfo();
		UsernameChanged?.Invoke(null);
	}

	public void SignOut() {
		Auth.SignOut();

		UserInfo = null;
		UsernameChanged?.Invoke(null);
	}

	#endregion

	public async Task UpdateUsername(string username) {
		username = username.Trim();
		await UserReference.Child(nameof(UserInfo.username)).SetValueAsync(username);
		UserInfo.username = username;

		UsernameChanged?.Invoke(username);
	}

	public async Task<UserInfo> GetUserInfo(string userId) {
		DataSnapshot snap = await RootReference.Child("users").Child(userId).GetValueAsync();
		UserInfo userInfo = JsonUtility.FromJson<UserInfo>(snap.GetRawJsonValue());
		userInfo.id = snap.Key;
		return userInfo;
	}

	private async Task FetchUserInfo() {
		DataSnapshot snap = await UserReference.GetValueAsync();
		UserInfo = JsonUtility.FromJson<UserInfo>(snap.GetRawJsonValue() ?? "{}");
		UserInfo.id = snap.Key;
	}

	private async Task PushUserInfo() {
		string userInfoJson = JsonUtility.ToJson(UserInfo);
		await UserReference.SetRawJsonValueAsync(userInfoJson);
	}
}