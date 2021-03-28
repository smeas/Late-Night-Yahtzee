using UnityEngine;
using Utilities;

namespace Other {
	public class KeepScreenOn : SingletonBehaviour<KeepScreenOn> {
		private void OnEnable() {
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
		}

		private void OnDisable() {
			Screen.sleepTimeout = SleepTimeout.SystemSetting;
		}
	}
}