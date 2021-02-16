using System;

namespace UI.Modal {
	public class InfoModal : ModalBase<Action> {
		public void Ok() {
			if (!isShowing) return;

			Hide();
			resultCallback?.Invoke();
			resultCallback = null;
		}
	}
}