using System;

namespace UI.Modal {
	public class YesNoModal : ModalBase<Action<bool>> {
		public void Yes() {
			ReturnResult(true);
		}

		public void No() {
			ReturnResult(false);
		}

		private void ReturnResult(bool result) {
			if (!isShowing)
				return;

			Hide();
			resultCallback?.Invoke(result);
			resultCallback = null;
		}
	}

}