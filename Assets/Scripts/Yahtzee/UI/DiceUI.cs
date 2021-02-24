using UnityEngine;
using UnityEngine.UI;

namespace Yahtzee.UI {
	public class DiceUI : MonoBehaviour {
		[SerializeField] private Image[] diceImages;
		[SerializeField] private Sprite[] diceSprites;

		public DiceSet Dice { get; set; }

		public void UpdateRepresentation() {
			Debug.Assert(diceImages.Length == Dice.Dice.Length);
			Debug.Assert(diceSprites.Length == 6);

			for (int i = 0; i < diceImages.Length; i++) {
				diceImages[i].sprite = diceSprites[Dice.Dice[i].Value - 1];
			}
		}
	}
}