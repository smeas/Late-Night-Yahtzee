using Audio;
using UnityEngine;

namespace Yahtzee.UI {
	public class ThrowableDie : MonoBehaviour {
		// Maps a Direction3Ds to the physical numbers on the dice.
		private static readonly int[] dieNumbers = {
			4, 3,
			1, 6,
			5, 2,
		};

		[SerializeField] private RandomAudioPlayer audioPlayer;

		private Rigidbody rigi;
		private bool locked;

		public Vector3 DefaultPosition { get; private set; }

		public DiceUI3D DiceUI { get; set; }

		private void Awake() {
			rigi = GetComponent<Rigidbody>();
			DefaultPosition = transform.position;
		}

		private void OnMouseDown() {
			if (DiceUI != null)
				DiceUI.OnDiePressed(this);
		}

		private void OnCollisionEnter(Collision other) {
			if (other.relativeVelocity.sqrMagnitude > 0.1f && other.collider.CompareTag("Table"))
				audioPlayer.Play();
		}

		public bool IsStable => rigi.IsSleeping();

		public Direction3D SideUp {
			get {
				Transform self = transform;

				float up = Vector3.Dot(self.up, Vector3.up);
				float right = Vector3.Dot(self.right, Vector3.up);
				float forward = Vector3.Dot(self.forward, Vector3.up);
				float absUp = Mathf.Abs(up);
				float absRight = Mathf.Abs(right);
				float absForward = Mathf.Abs(forward);

				if (absUp >= absRight) {
					if (absUp >= absForward) // up
						return up >= 0 ? Direction3D.Up : Direction3D.Down;
					else // forward
						return forward >= 0 ? Direction3D.Forward : Direction3D.Back;
				}
				else { // up < right
					if (absRight >= absForward) // right
						return right >= 0 ? Direction3D.Right : Direction3D.Left;
					else // forward
						return forward >= 0 ? Direction3D.Forward : Direction3D.Back;
				}
			}
		}

		public Vector3 RealignedEulerRotation {
			get {
				Vector3 worldEuler = transform.eulerAngles;
				worldEuler.y = RoundToNearestMultiple(worldEuler.y, 90f);
				return worldEuler;
			}
		}

		public int CurrentValue => dieNumbers[(int)SideUp];

		public bool Locked {
			get => locked;
			set {
				locked = value;

				// Freeze the rigidbody when locked
				rigi.isKinematic = locked;
			}
		}

		public void Throw(Vector3 velocity, Vector3 rotation) {
			rigi.velocity = velocity;
			rigi.angularVelocity = rotation;
		}

		public void ClearVelocity() {
			rigi.velocity = Vector3.zero;
			rigi.angularVelocity = Vector3.zero;
		}

		private static float RoundToNearestMultiple(float value, float multiple) {
			float remainder = value % multiple;
			if (remainder >= multiple * 0.5f)
				return value + (multiple - remainder);

			return value - remainder;
		}
	}
}