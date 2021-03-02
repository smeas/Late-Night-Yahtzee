using UnityEngine;

namespace Yahtzee.UI {
	public class ThrowableDie : MonoBehaviour {
		// Maps a Direction3D to the number on the die.
		private static readonly int[] dieNumbers = {
			4, 3,
			1, 6,
			5, 2,
		};

		private Rigidbody rigi;

		public bool IsStable =>
			rigi.IsSleeping(); //|| rigi.velocity.sqrMagnitude < 0.1f && rigi.angularVelocity.sqrMagnitude < 0.1f;

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

		public int CurrentValue => dieNumbers[(int)SideUp];

		private void Awake() {
			rigi = GetComponent<Rigidbody>();
		}

		// private void Update() {
		// 	print($"{numbers[(int)SideUp]} -- {SideUp}");
		// }

		public void Throw(Vector3 velocity, Vector3 rotation) {
			rigi.velocity = velocity;
			rigi.angularVelocity = rotation;
		}
	}
}