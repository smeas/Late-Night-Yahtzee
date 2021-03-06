using UnityEngine;

namespace Other {
	public class KeepScreenOn : MonoBehaviour {
		private void OnEnable() {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		private void OnDisable() {
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}
	}
}