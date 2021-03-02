using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Yahtzee.UI {
	public class DiceUI3D : MonoBehaviour {
		[SerializeField] private ThrowableDie[] dice;

		[SerializeField] private Vector3 throwDirection = Vector3.up;
		[SerializeField] private float throwConeAngle = 30;
		[SerializeField] private float throwStrength = 5;

		[SerializeField, MinMaxRange(0, 100)]
		private Vector2 rotationStrength = new Vector2(5, 10);

		public bool CanLock { get; set; }
		public bool CanRoll { get; set; }
		public bool BlankDice { get; set; }

		private YahtzeeGame game;
		private DiceSet diceSet;

		public void Initialize(YahtzeeGame game, DiceSet diceSet) {
			this.game = game;
			this.diceSet = diceSet;
		}

		public void UpdateRepresentation() {
			throw new NotImplementedException();
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.R)) {
				RollDice();
			}
		}

		private void RollDice() {
			StopAllCoroutines();
			StartCoroutine(CoRollDice());
		}

		private Vector3 GetRandomThrowVector() {
			float radius = Mathf.Tan(throwConeAngle * Mathf.Deg2Rad) * throwStrength;
			Vector2 pointInCircle = Random.insideUnitCircle * radius;
			return Quaternion.LookRotation(throwDirection) * new Vector3(pointInCircle.x, pointInCircle.y, throwStrength);
		}

		private IEnumerator CoRollDice() {
			// Throw the dice
			foreach (ThrowableDie die in dice) {
				die.Throw(GetRandomThrowVector(),
				          Random.insideUnitSphere * Random.Range(rotationStrength.x, rotationStrength.y));
			}

			// Wait for all of the dice to stabilize
			while (true) {
				yield return new WaitForFixedUpdate();

				if (dice.All(die => die.IsStable))
					break;
			}

			print("Rolled: " + string.Join(", ", dice.Select(x => x.CurrentValue)));
		}
	}
}