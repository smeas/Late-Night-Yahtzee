using System;
using TMPro;
using UnityEngine;

namespace UI.Modal {
	public abstract class ModalBase<TCallback> : MonoBehaviour where TCallback : Delegate {
		[SerializeField] protected TextMeshProUGUI contentText;

		protected bool isShowing;
		protected TCallback resultCallback;

		private void Awake() {
			gameObject.SetActive(false);
		}

		public void Show(string message, TCallback callback) {
			if (isShowing) return;

			contentText.text = message;
			resultCallback = callback;

			gameObject.SetActive(true);
			isShowing = true;
		}

		public void Hide() {
			gameObject.SetActive(false);
			isShowing = false;
		}
	}
}