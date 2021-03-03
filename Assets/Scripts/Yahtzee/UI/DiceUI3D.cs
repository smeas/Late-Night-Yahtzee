using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
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

		[Space]
		[SerializeField] private float moveBackDuration = 0.5f;
		[SerializeField] private Ease moveBackEase = Ease.OutQuad;

		[SerializeField] private float lockMoveDuration = 0.2f;
		[SerializeField] private Ease lockMoveEase = Ease.OutQuad;
		[SerializeField] private Vector3 lockMoveOffset;

		private YahtzeeGame game;
		private DiceSet diceSet;
		private bool canLock = true;
		private bool isRolling;

		public bool CanLock {
			get => canLock;
			set {
				canLock = value;

				if (!canLock) {
					foreach (ThrowableDie die in dice)
						die.Locked = false;
				}
			}
		}

		public bool CanRoll { get; set; } = true;
		public bool BlankDice { get; set; }

		private void Start() {
			foreach (ThrowableDie die in dice) {
				die.DiceUI = this;
			}
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.R)) {
				RollDice();
			}
		}

		public void Initialize(YahtzeeGame game, DiceSet diceSet) {
			this.game = game;
			this.diceSet = diceSet;

			Debug.Assert(dice.Length == diceSet.Dice.Length);
		}

		public void UpdateRepresentation() {
			throw new NotImplementedException();
		}

		public void OnDiePressed(ThrowableDie die) {
			// int index = Array.IndexOf(dice, die);
			// if (index == -1)
			// 	return;

			if (!isRolling && CanLock)
				SetLocked(die, !die.Locked);
		}

		private void RollDice() {
			if (!CanRoll) return;

			StopAllCoroutines();
			StartCoroutine(CoRollDice());
		}

		private Vector3 GetRandomThrowVector() {
			float radius = Mathf.Tan(throwConeAngle * Mathf.Deg2Rad) * throwStrength;
			Vector2 pointInCircle = Random.insideUnitCircle * radius;
			return Quaternion.LookRotation(throwDirection) * new Vector3(pointInCircle.x, pointInCircle.y, throwStrength);
		}

		private IEnumerator CoRollDice() {
			isRolling = true;

			// Throw the dice
			foreach (ThrowableDie die in dice) {
				if (die.Locked) continue;

				die.Throw(GetRandomThrowVector(),
				          Random.insideUnitSphere * Random.Range(rotationStrength.x, rotationStrength.y));
			}

			// Wait for all the dice to land
			while (true) {
				yield return new WaitForFixedUpdate();

				if (dice.All(die => die.Locked || die.IsStable))
					break;
			}

			// Move the dice back into their default position
			foreach (ThrowableDie die in dice) {
				if (die.Locked) continue;

				die.transform
					.DOMove(die.DefaultPosition, moveBackDuration)
					.SetEase(moveBackEase);
			}

			yield return new WaitForSeconds(moveBackDuration);

			isRolling = false;
			print("Rolled: " + string.Join(", ", dice.Select(x => x.CurrentValue)));
		}

		private void SetLocked(ThrowableDie die, bool state) {
			if (die.Locked == state) return;

			Vector3 targetPosition = die.DefaultPosition;
			if (state)
				targetPosition += lockMoveOffset;

			die.transform.DOMove(targetPosition, lockMoveDuration).SetEase(lockMoveEase);

			die.Locked = state;
		}
	}
}