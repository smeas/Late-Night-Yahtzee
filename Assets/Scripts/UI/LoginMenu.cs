using System;
using Firebase;
using Firebase.Auth;
using TMPro;
using UI.Modal;
using UnityEngine;
using UnityEngine.Events;

namespace UI {
	public class LoginMenu : MonoBehaviour {
		[SerializeField] private TMP_InputField emailField;
		[SerializeField] private TMP_InputField passwordField;

		[Header("Events")]
		[SerializeField] private UnityEvent onSuccessfulLogin;

		public async void Register() {
			try {
				await FirebaseManager.Instance.RegisterUser(emailField.text, passwordField.text);
			}
			catch (AggregateException agg) when (agg.InnerException is FirebaseException e) {
				if (e.ErrorCode == (int)AuthError.EmailAlreadyInUse) {
					// Existing account.
					ModalManager.Instance.ShowYesNo(
						"An account with that email already exists. Do you want to sign in?",
						ok => {
							if (ok) {
								SignIn();
							}
						});
				}
				else {
					Debug.LogError($"Registration failed: {e}  ({e.ErrorCode})");
					ModalManager.Instance.ShowInfo(e.Message, null);
				}

				return;
			}

			Debug.Assert(FirebaseManager.Instance.User != null);
			Debug.Log("Registered successfully");
			onSuccessfulLogin.Invoke();
		}

		public async void SignIn() {
			try {
				await FirebaseManager.Instance.SignInUser(emailField.text, passwordField.text);
			}
			catch (AggregateException agg) when (agg.InnerException is FirebaseException e) {
				if (e.ErrorCode == (int)AuthError.UserNotFound) {
					// User not registered.
					ModalManager.Instance.ShowYesNo(
						"A account with that email does not exist. Would you like to create one?",
						ok => {
							if (ok) {
								Register();
							}
						});
				}
				else {
					Debug.LogError($"Sign in failed: {e}  ({e.ErrorCode})");
					ModalManager.Instance.ShowInfo(e.Message, null);
				}

				return;
			}

			Debug.Assert(FirebaseManager.Instance.User != null);
			Debug.Log("Signed in successfully");
			onSuccessfulLogin.Invoke();
		}

		public async void SignInAnonymously() {
			try {
				await FirebaseManager.Instance.SignInAnonymously();
			}
			catch (AggregateException agg) when (agg.InnerException is FirebaseException e) {
				Debug.LogError($"Anonymous sign in failed: {e}  ({e.ErrorCode})");
				ModalManager.Instance.ShowInfo(e.Message, null);

				return;
			}

			Debug.Assert(FirebaseManager.Instance.User != null);
			Debug.Log("Signed in successfully");
			onSuccessfulLogin.Invoke();
		}
	}
}