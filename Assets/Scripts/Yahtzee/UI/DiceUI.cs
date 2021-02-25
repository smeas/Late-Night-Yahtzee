using UnityEngine;
using UnityEngine.UI;

namespace Yahtzee.UI {
	[DefaultExecutionOrder(1)]
	public class DiceUI : MonoBehaviour {
		[SerializeField] private Button[] diceButtons;
		[SerializeField] private Sprite[] diceSprites;
		[SerializeField] private Sprite blankDieSprite;
		[SerializeField] private Button rollButton;

		private YahtzeeGame game;
		private DiceSet diceSet;
		private bool canLock;
		private bool canRoll;
		private bool blankDice;

		public bool CanLock {
			set => canLock = value;
		}

		public bool CanRoll {
			set => rollButton.interactable = canRoll = value;
		}

		public bool BlankDice {
			set => blankDice = value;
		}

		private void Start() {
			Debug.Assert(diceButtons.Length == diceSet.Dice.Length, "diceButtons.Length == Dice.Dice.Length");
			Debug.Assert(diceSprites.Length == 6, "diceSprites.Length == 6");

			for (int i = 0; i < 6; i++) {
				int index = i;
				diceButtons[i].onClick.AddListener(() => OnDieClick(index));
			}
		}

		public void Initialize(YahtzeeGame game, DiceSet diceSet) {
			this.game = game;
			this.diceSet = diceSet;
		}

		public void UpdateRepresentation() {
			for (int i = 0; i < diceButtons.Length; i++) {
				diceButtons[i].GetComponent<Image>().sprite = blankDice ? blankDieSprite : diceSprites[diceSet.Dice[i].Value - 1];
				UpdateLockEffect(i);
			}
		}

		public void OnRollClick() {
			if (canRoll)
				game.OnRollPressed();
		}

		private void OnDieClick(int index) {
			if (!canLock) return;
			diceSet.Dice[index].Locked = !diceSet.Dice[index].Locked;
			UpdateLockEffect(index);
		}

		private void UpdateLockEffect(int dieIndex) {
			bool locked = diceSet.Dice[dieIndex].Locked;
			diceButtons[dieIndex].transform.localScale = locked ? new Vector3(0.8f, 0.8f, 0.8f) : Vector3.one;
			diceButtons[dieIndex].GetComponent<Image>().color = locked ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
		}
	}
}