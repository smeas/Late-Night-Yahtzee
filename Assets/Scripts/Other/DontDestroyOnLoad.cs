using UnityEngine;

namespace Other {
	public class DontDestroyOnLoad : MonoBehaviour {
		private void Start() {
			DontDestroyOnLoad(gameObject);
		}
	}
}