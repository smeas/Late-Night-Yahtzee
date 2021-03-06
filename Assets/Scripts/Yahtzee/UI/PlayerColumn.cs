using TMPro;
using UnityEngine;

namespace Yahtzee.UI {
	public class PlayerColumn : MonoBehaviour {
		[SerializeField] private TextMeshProUGUI headerText;
		[SerializeField] private TextMeshProUGUI upperSumText;
		[SerializeField] private TextMeshProUGUI totalSumText;

		[Space]
		[SerializeField] private Color normalTextColor = Color.black;
		[SerializeField] private Color hintTextColor = Color.gray;

		private YahtzeeGame game;
		private PlayerIndex playerIndex;
		private ScoreItem[] scoreItems;

		public string Name { set => headerText.text = value; }
		public PlayerState Data { get; set; }
		public int[] CurrentRollScores { get; set; }
		public bool ShowHints { get; set; }

		private void Awake() {
			scoreItems = GetComponentsInChildren<ScoreItem>();
			Debug.Assert(scoreItems.Length == 16, "scoreItems.Length == 16");

			foreach (ScoreItem item in scoreItems) {
				item.text.text = "";
			}

			headerText.text = gameObject.name;
			upperSumText.text = "0";
			totalSumText.text = "0";
		}

		public void Initialize(YahtzeeGame game, PlayerIndex playerIndex) {
			this.game = game;
			this.playerIndex = playerIndex;
		}

		public void UpdateRepresentation() {
			for (int i = 0; i < Data.Scores.Length; i++) {
				TextMeshProUGUI text = scoreItems[i].text;
				text.color = normalTextColor;
				text.fontStyle = FontStyles.Normal;

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
						text.fontStyle = FontStyles.Italic;
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