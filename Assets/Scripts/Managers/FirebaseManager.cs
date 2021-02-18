using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class FirebaseManager : SingletonBehaviour<FirebaseManager> {
	public FirebaseDatabase Database { get; private set; }
	public FirebaseAuth Auth { get; private set; }
	public FirebaseUser User => Auth.CurrentUser;
	public string UserId => User.UserId;
	public DatabaseReference RootReference => Database.RootReference;
	public DatabaseReference UserReference => Database.RootReference.Child("users/" + User.UserId);
	public bool IsSignedIn => User != null;
	public bool Initialized { get; private set; }

	private Task initTask;

	protected override void Awake() {
		base.Awake();

		DontDestroyOnLoad(this);

		initTask = Initialize();
	}

	private async Task Initialize() {
		await FirebaseApp.CheckAndFixDependenciesAsync();
		Debug.Log("Firebase initialized");

		Database = FirebaseDatabase.DefaultInstance;
		Auth = FirebaseAuth.DefaultInstance;

		// // Sign in
		// FirebaseUser user = await Auth.SignInAnonymouslyAsync();
		// UserReference = Database.RootReference.Child("users/" + user.UserId);
		// Debug.Log($"Signed in with user id: {user.UserId}");

		Initialized = true;
		initTask = null;
	}

	public async Task WaitForInitialization() {
		if (!Initialized)
			await initTask;
	}

	public async Task RegisterUser(string email, string password) {
		if (!Initialized)
			await initTask;

		await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
	}

	public async Task SignInUser(string email, string password) {
		if (!Initialized)
			await initTask;

		await Auth.SignInWithEmailAndPasswordAsync(email, password);
	}

	public async Task SignInAnonymously() {
		if (!Initialized)
			await initTask;

		await Auth.SignInAnonymouslyAsync();
	}

	public void SignOut() {
		Auth.SignOut();
	}

	public async Task<string> LoadUserData() {
		if (!Initialized)
			await initTask;

		DataSnapshot data = await UserReference.Child("data").GetValueAsync();
		return data.GetRawJsonValue();
	}

	public async Task SaveUserData(string data) {
		if (!Initialized)
			await initTask;

		await UserReference.Child("data").SetRawJsonValueAsync(data);
	}
}