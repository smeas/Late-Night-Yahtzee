using System;
using TMPro;
using UnityEngine;

namespace UI.Modal {
	public class ValueModal : ModalBase<Action<string>> {
		[SerializeField] private TMP_InputField inputField;

		public void Show(string message, string placeholder, Action<string> callback) {
			if (isShowing) return;

			((TextMeshProUGUI)inputField.placeholder).text = placeholder;
			base.Show(message, callback);
		}

		public void Ok() {
			if (!isShowing) return;

			Hide();
			resultCallback?.Invoke(inputField.text);
			resultCallback = null;
		}
	}
}