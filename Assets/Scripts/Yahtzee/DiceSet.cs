using System;
using System.Linq;
using Random = UnityEngine.Random;

namespace Yahtzee {
	public class DiceSet {
		private const int DiceCount = 5;

		private readonly Die[] dice = new Die[DiceCount];

		public Die[] Dice => dice;

		public DiceSet() {
			for (int i = 0; i < dice.Length; i++) {
				dice[i] = new Die();
			}
		}

		public DiceSet(params int[] values) {
			if (values.Length != DiceCount)
				throw new ArgumentException();

			for (int i = 0; i < dice.Length; i++) {
				dice[i] = new Die {Value = values[i]};
			}
		}

		public void Roll() {
			foreach (Die die in dice) {
				if (!die.Locked) {
					die.Value = Random.Range(1, 7);
				}
			}
		}

		public int[] CalculateScores() {
			// DO NOT REORDER!!!
			return new int[16] {
				GetNumberScore(1),
				GetNumberScore(2),
				GetNumberScore(3),
				GetNumberScore(4),
				GetNumberScore(5),
				GetNumberScore(6),
				0, // Bonus
				GetMultipleScore(2),
				GetTwoPairScore(),
				GetMultipleScore(3),
				GetMultipleScore(4),
				GetFullHouseScore(),
				GetSmallStraightScore(),
				GetLargeStraightScore(),
				GetChanceScore(),
				GetYahtzeeScore(),
			};
		}

		// Aces, Twos, Threes, Fours, Fives, Sixes
		public int GetNumberScore(int number) {
			return dice.Count(die => die.Value == number) * number;
		}

		// Pair, ThreeOfAKind, FourOfAKind
		public int GetMultipleScore(int count) {
			int[] cv = CountValues();

			for (int i = cv.Length - 1; i >= 0; i--) {
				if (cv[i] >= count)
					return (i + 1) * count;
			}

			return 0;
		}

		public int GetTwoPairScore() {
			int[] cv = CountValues();
			int firstValue = 0;
			int secondValue = 0;

			for (int i = cv.Length - 1; i >= 0; i--) {
				if (cv[i] >= 2) {
					if (firstValue == 0) {
						firstValue = i + 1;
					}
					else {
						secondValue = i + 1;
						break;
					}
				}
			}

			if (firstValue != 0 && secondValue != 0)
				return firstValue * 2 + secondValue * 2;

			return 0;
		}

		public int GetFullHouseScore() {
			int[] cv = CountValues();
			int twoValue = 0;
			int threeValue = 0;

			for (int i = 0; i < cv.Length; i++) {
				if (cv[i] == 2)
					twoValue = i + 1;
				else if (cv[i] == 3)
					threeValue = i + 1;
			}

			if (twoValue != 0 && threeValue != 0)
				return twoValue * 2 + threeValue * 3;

			return 0;
		}

		public int GetSmallStraightScore() {
			int[] cv = CountValues();
			if (cv[1 - 1] != 0 &&
				cv[2 - 1] != 0 &&
				cv[3 - 1] != 0 &&
				cv[4 - 1] != 0 &&
				cv[5 - 1] != 0)
				return 15;

			return 0;
		}

		public int GetLargeStraightScore() {
			int[] cv = CountValues();
			if (cv[2 - 1] != 0 &&
				cv[3 - 1] != 0 &&
				cv[4 - 1] != 0 &&
				cv[5 - 1] != 0 &&
				cv[6 - 1] != 0)
				return 20;

			return 0;
		}

		public int GetChanceScore() {
			return dice.Sum(x => x.Value);
		}

		public int GetYahtzeeScore() {
			if (dice.All(x => x.Value == dice[0].Value))
				return 50;

			return 0;
		}

		private int[] CountValues() {
			int[] cv = new int[6];
			for (int i = 0; i < DiceCount; i++)
				cv[dice[i].Value - 1]++;

			return cv;
		}
	}
}