using UnityEngine;
using UnityEngine.UI;

namespace Yahtzee.UI {
	public class Sheet : MonoBehaviour {
		[SerializeField] private GameObject categoryButtonsSource;
		[SerializeField] private PlayerColumn playerColumnPrefab;
		[SerializeField] private Transform playersRoot;

		private YahtzeeGame game;

		public PlayerColumn[] PlayerColumns { get; private set; }

		private void Start() {
			Button[] buttons = categoryButtonsSource.GetComponentsInChildren<Button>();
			Debug.Assert(buttons.Length == 16);

			for (int i = 0; i < buttons.Length; i++) {
				Category category = (Category)i;
				buttons[i].onClick.AddListener(() => OnCategoryPressed(category));
			}
		}

		public void Initialize(YahtzeeGame game, int numberOfPlayers, string[] names, PlayerState[] states) {
			this.game = game;

			PlayerColumns = new PlayerColumn[numberOfPlayers];
			for (int i = 0; i < numberOfPlayers; i++) {
				PlayerColumns[i] = Instantiate(playerColumnPrefab, playersRoot);
				PlayerColumns[i].Name = names[i];
				PlayerColumns[i].Data = states[i];
				PlayerColumns[i].UpdateRepresentation();
			}
		}

		private void OnCategoryPressed(Category category) {
			if (game == null) return;
			game.OnCategoryPressed(category);
		}
	}
}