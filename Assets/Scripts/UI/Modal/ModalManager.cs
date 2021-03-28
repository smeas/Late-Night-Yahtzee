using System;
using UnityEngine;
using Utilities;

namespace UI.Modal {
	[RequireComponent(typeof(Canvas))]
	public class ModalManager : SingletonBehaviour<ModalManager> {
		[SerializeField] private YesNoModal yesNoModalPrefab;
		[SerializeField] private InfoModal infoModalPrefab;
		[SerializeField] private ValueModal valueModalPrefab;

		[Space]
		[SerializeField] private GameObject backgroundGraphic;

		private Canvas canvas;
		private YesNoModal yesNoModal;
		private InfoModal infoModal;
		private ValueModal valueModal;

		public bool IsShowing { get; private set; }

		protected override void Awake() {
			base.Awake();
			canvas = GetComponent<Canvas>();
			backgroundGraphic.SetActive(false);
		}

		public void ShowYesNo(string message, Action<bool> callback) {
			if (IsShowing) {
				Debug.LogError("Attempt to show multiple modals.");
				return;
			}

			if (yesNoModal == null)
				yesNoModal = Instantiate(yesNoModalPrefab, canvas.transform);

			IsShowing = true;
			backgroundGraphic.SetActive(true);
			yesNoModal.Show(message, result => {
				IsShowing = false;
				backgroundGraphic.SetActive(false);
				callback?.Invoke(result);
			});
		}

		public void ShowInfo(string message, Action callback) {
			if (IsShowing) {
				Debug.LogError("Attempt to show multiple modals.");
				return;
			}

			if (infoModal == null)
				infoModal = Instantiate(infoModalPrefab, canvas.transform);

			IsShowing = true;
			backgroundGraphic.SetActive(true);
			infoModal.Show(message, () => {
				IsShowing = false;
				backgroundGraphic.SetActive(false);
				callback?.Invoke();
			});
		}

		public void ShowValue(string message, string placeholder, Action<string> callback) {
			if (IsShowing) {
				Debug.LogError("Attempt to show multiple modals.");
				return;
			}

			if (valueModal == null)
				valueModal = Instantiate(valueModalPrefab, canvas.transform);

			IsShowing = true;
			backgroundGraphic.SetActive(true);
			valueModal.Show(message, placeholder, result => {
				IsShowing = false;
				backgroundGraphic.SetActive(false);
				callback?.Invoke(result);
			});
		}
	}
}