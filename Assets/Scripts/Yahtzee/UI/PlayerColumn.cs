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
		[SerializeField] private Color lastModifiedHighlightColor = Color.yellow;

		private ScoreItem[] scoreItems;
		private ScoreItem lastModifiedHighlight;

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

		public void UpdateRepresentation() {
			if (lastModifiedHighlight != null)
				lastModifiedHighlight.background.color = Color.clear;

			if (Data.LastModified != -1) {
				lastModifiedHighlight = scoreItems[Data.LastModified];
				lastModifiedHighlight.background.color = lastModifiedHighlightColor;
			}

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