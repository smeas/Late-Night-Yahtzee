using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yahtzee.UI {
	public class PlayerColumn : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI headerText;
		[SerializeField] private TextMeshProUGUI upperSumText;
		[SerializeField] private TextMeshProUGUI totalSumText;
		[SerializeField] private Button[] scoreButtons;

		[Space]
		[SerializeField] private Color normalTextColor = Color.black;
		[SerializeField] private Color hintTextColor = Color.gray;

		private YahtzeeGame game;
		private PlayerIndex playerIndex;

		public string Name { set => headerText.text = value; }
		public PlayerState Data { get; set; }
		public int[] CurrentRollScores { get; set; }
		public bool ShowHints { get; set; }

		private void Awake() {
			Debug.Assert(scoreButtons.Length == 16, "scoreButtons.Length == 16");

			foreach (Button button in scoreButtons) {
				TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>();
				text.text = "";
			}

			headerText.text = gameObject.name;
			upperSumText.text = "0";
			totalSumText.text = "0";
		}

		public void Initialize(YahtzeeGame game, PlayerIndex playerIndex) {
			this.game = game;
			this.playerIndex = playerIndex;

			for (int i = 0; i < scoreButtons.Length; i++) {
				int index = i;
				scoreButtons[i].onClick.AddListener(() => game.OnScorePressed(playerIndex, (Category)index));
			}
		}

		public void UpdateRepresentation() {
			for (int i = 0; i < Data.Scores.Length; i++) {
				TextMeshProUGUI text = scoreButtons[i].GetComponentInChildren<TextMeshProUGUI>();
				text.color = normalTextColor;

				if (Data.HasScratched((Category)i)) {
					text.text = "-";
				}
				else if (Data.Scores[i] == 0) {
					if ((Category)i == Category.Bonus && Data.HasGottenBonus) {
						text.text = "0";
					}
					else if (ShowHints) {
						text.text = CurrentRollScores[i] != 0 ? CurrentRollScores[i].ToString() : "-";
						text.color = hintTextColor;
					}
					else {
						text.text = "";
					}
				}
				else {
					text.text = Data.Scores[i].ToString();
				}
			}

			upperSumText.text = Data.UpperSum.ToString();
			totalSumText.text = Data.TotalSum.ToString();
		}
	}
}