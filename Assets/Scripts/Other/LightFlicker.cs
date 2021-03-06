using UnityEngine;

namespace Other {
	public class LightFlicker : MonoBehaviour {
		[SerializeField] private float flickerAmount = 10;
		[SerializeField] private float flickerSpeed = 10;

		private new Light light;
		private float intensity;
		private float timer;

		private void Start() {
			light = GetComponent<Light>();
			intensity = light.intensity;
		}

		private void Update() {
			float speed = 1f / flickerSpeed;
			float amount = flickerAmount / 2f;

			timer += Time.deltaTime;
			if (timer >= speed) {
				timer -= speed;
				light.intensity = Mathf.Max(0, Random.Range(intensity - amount, intensity + amount));
			}
		}
	}
}