using UnityEngine;

namespace Other {
	public class LightFlicker : MonoBehaviour {
		[SerializeField] private float flickerAmount = 10;
		[SerializeField] private float flickerSpeed = 10;

		private Light lamp;
		private float intensity;
		private float timer;

		private void Start() {
			lamp = GetComponent<Light>();
			intensity = lamp.intensity;
		}

		private void Update() {
			float speed = 1f / flickerSpeed;
			float amount = flickerAmount / 2f;

			timer += Time.deltaTime;
			if (timer >= speed) {
				timer -= speed;
				lamp.intensity = Mathf.Max(0, Random.Range(intensity - amount, intensity + amount));
			}
		}
	}
}