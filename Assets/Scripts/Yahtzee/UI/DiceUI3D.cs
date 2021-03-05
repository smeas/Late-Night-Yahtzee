using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Yahtzee.UI {
	public class DiceUI3D : MonoBehaviour, IDiceUI {
		[SerializeField] private ThrowableDie[] dice;
		[SerializeField] private Button rollButton;

		[SerializeField] private Vector3 throwDirection = Vector3.up;
		[SerializeField] private float throwConeAngle = 30;
		[SerializeField] private float throwStrength = 5;

		[SerializeField, MinMaxRange(0, 100)]
		private Vector2 rotationStrength = new Vector2(5, 10);

		[SerializeField] private float maxThrowDuration = 5f;

		[Space]
		[SerializeField] private float moveBackDuration = 0.5f;
		[SerializeField] private Ease moveBackEase = Ease.OutQuad;

		[SerializeField] private float lockMoveDuration = 0.2f;
		[SerializeField] private Ease lockMoveEase = Ease.OutQuad;
		[SerializeField] private Vector3 lockMoveOffset;

		private YahtzeeGame game;
		private DiceSet diceSet;
		private bool canLock = true;
		private bool canRoll = true;
		private bool isRolling;

		public event Action RollCompleted;

		public bool CanLock {
			get => canLock;
			set {
				canLock = value;

				if (!canLock) {
					foreach (ThrowableDie die in dice)
						SetLocked(die, false);
				}
			}
		}

		public bool CanRoll {
			get => canRoll;
			set => rollButton.interactable = canRoll = value;
		}

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

		public void UpdateRepresentation() { }

		public void OnDiePressed(ThrowableDie die) {
			if (!isRolling && CanLock)
				SetLocked(die, !die.Locked);
		}

		public void RollDice() {
			if (!CanRoll) return;

			StopAllCoroutines();
			StartCoroutine(CoRollDice());
		}

		private Vector3 GetRandomThrowVector() {
			float radius = Mathf.Tan(throwConeAngle * Mathf.Deg2Rad) * throwStrength;
			Vector2 pointInCircle = Random.insideUnitCircle * radius;
			return Quaternion.LookRotation(throwDirection.normalized) *
				new Vector3(pointInCircle.x, pointInCircle.y, throwStrength);
		}

		private IEnumerator CoRollDice() {
			isRolling = true;

			// Throw the dice
			foreach (ThrowableDie die in dice) {
				if (die.Locked) continue;

				die.Throw(GetRandomThrowVector(),
				          Random.insideUnitSphere * Random.Range(rotationStrength.x, rotationStrength.y));
			}

			float timeout = maxThrowDuration;

			// Wait for all the dice to land
			while (true) {
				yield return new WaitForFixedUpdate();

				timeout -= Time.fixedDeltaTime;
				if (timeout <= 0) {
					foreach (ThrowableDie die in dice)
						die.ClearVelocity();

					break;
				}

				if (dice.All(die => die.Locked || die.IsStable))
					break;
			}

			// Move the dice back into their default position
			foreach (ThrowableDie die in dice) {
				if (die.Locked) continue;

				die.transform
					.DOMove(die.DefaultPosition, moveBackDuration)
					.SetEase(moveBackEase);

				die.transform
					.DORotate(die.RealignedEulerRotation, moveBackDuration)
					.SetEase(moveBackEase);
			}

			yield return new WaitForSeconds(moveBackDuration);

			EndRoll();
		}

		private void EndRoll() {
			print("Rolled: " + string.Join(", ", dice.Select(x => x.CurrentValue)));
			isRolling = false;

			if (!CanRoll) return;

			// Update the dice set with the new values
			for (int i = 0; i < diceSet.Dice.Length; i++)
				diceSet.Dice[i].Value = dice[i].CurrentValue;

			RollCompleted?.Invoke();
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