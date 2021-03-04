using System;

namespace Yahtzee.UI {
	public interface IDiceUI {
		event Action RollCompleted;
		bool CanLock { set; }
		bool CanRoll { set; }
		bool BlankDice { set; }
		void Initialize(YahtzeeGame game, DiceSet diceSet);
		void UpdateRepresentation();
		void RollDice();
	}
}