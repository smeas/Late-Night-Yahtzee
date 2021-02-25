using UI.Modal;
using UnityEngine;
using UnityEngine.Events;

namespace UI {
	public class BackEvent : MonoBehaviour {
		[SerializeField] private bool ignoreIfModalIsShowing;
		[SerializeField] private UnityEvent onBack;

		private void Update() {
			if (ignoreIfModalIsShowing && ModalManager.Instance.IsShowing) return;
			if (Input.GetKeyDown(KeyCode.Escape)) {
				onBack.Invoke();
			}
		}
	}
}