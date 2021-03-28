using System;
using UnityEngine;
using Utilities;

namespace Other {
	public class ShakeDetector : SingletonBehaviour<ShakeDetector> {
		// Adapted from: https://stackoverflow.com/questions/31389598/how-can-i-detect-a-shake-motion-on-a-mobile-device-using-unity3d-c-sharp/31389776
		private const float AccelerometerUpdateInterval = 1f / 60f;

		// The greater the value of LowPassKernelWidthInSeconds, the slower the
		// filtered value will converge towards current input sample (and vice versa).
		private const float LowPassKernelWidthInSeconds = 1f;

		[SerializeField] private float shakeDetectionThreshold = 4f;
		[SerializeField] private float minTimeBetweenShakes = 0.5f;

		private float lowPassFilterFactor;
		private Vector3 lowPassValue;
		private float shakeTimer;

		public event Action Shake;

		private void Start() {
			lowPassFilterFactor = AccelerometerUpdateInterval / LowPassKernelWidthInSeconds;
			shakeDetectionThreshold *= shakeDetectionThreshold;
			lowPassValue = Input.acceleration;
		}

		private void Update() {
			Vector3 acceleration = Input.acceleration;
			lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
			Vector3 deltaAcceleration = acceleration - lowPassValue;

			if (shakeTimer > 0f) {
				shakeTimer -= Time.unscaledDeltaTime;
				return;
			}

			if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold) {
				shakeTimer = minTimeBetweenShakes;
				Shake?.Invoke();
			}
		}
	}
}