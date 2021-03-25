using System;
using UnityEngine;

namespace Yahtzee {
	[Serializable]
	public class PlayerState {
		[SerializeField]
		private int[] scores = new int[16];

		[SerializeField]
		private int scratchedMask;

		[SerializeField]
		private int lastModified = -1;

		public int[] Scores => scores;
		public int UpperSum { get; private set; }
		public int TotalSum { get; private set; }
		public bool HasGottenBonus { get; private set; }
		public bool IsFinished { get; private set; }

		/// <summary>
		/// Bit-field with bit indices from <see cref="Category"/>.
		/// </summary>
		public int Scratched => scratchedMask;
		public int LastModified => lastModified;

		public int this[Category category] {
			get => scores[(int)category];
			set {
				if (scores[(int)category] == value)
					return;

				scores[(int)category] = value;

				// If we get the bonus, we want to highlight the move that got it, not the bonus itself
				if (category != Category.Bonus)
					lastModified = (int)category;
			}
		}

		public void Scratch(Category category) {
			int oldMask = scratchedMask;
			scratchedMask |= 1 << (int)category;

			if (scratchedMask != oldMask)
				lastModified = (int)category;
		}

		public bool HasScratched(Category category) {
			return (Scratched & (1 << (int)category)) != 0;
		}

		public bool HasScored(Category category) {
			return this[category] != 0;
		}

		public bool CanChoose(Category category) {
			return !HasScratched(category) && !HasScored(category);
		}

		public void UpdateSums() {
			UpperSum = this[Category.Ones] + this[Category.Twos] + this[Category.Threes] +
				this[Category.Fours] + this[Category.Fives] + this[Category.Sixes];

			if (UpperSum >= 63 && !HasScratched(Category.Bonus))
				this[Category.Bonus] = 50;
			else
				this[Category.Bonus] = 0;

			HasGottenBonus = !CanChoose(Category.Bonus) ||
				!CanChoose(Category.Ones) &&
				!CanChoose(Category.Twos) &&
				!CanChoose(Category.Threes) &&
				!CanChoose(Category.Fours) &&
				!CanChoose(Category.Fives) &&
				!CanChoose(Category.Sixes);

			TotalSum = UpperSum + this[Category.Bonus] + this[Category.Pair] + this[Category.TwoPair] +
				this[Category.ThreeOfAKind] + this[Category.FourOfAKind] + this[Category.FullHouse] +
				this[Category.SmallStraight] + this[Category.LargeStraight] + this[Category.Chance] +
				this[Category.Yahtzee];

			IsFinished =
				!CanChoose(Category.Ones) && !CanChoose(Category.Twos) && !CanChoose(Category.Threes) &&
				!CanChoose(Category.Fours) && !CanChoose(Category.Fives) && !CanChoose(Category.Sixes) &&
				!CanChoose(Category.Pair) && !CanChoose(Category.TwoPair) && !CanChoose(Category.ThreeOfAKind) &&
				!CanChoose(Category.FourOfAKind) && !CanChoose(Category.FullHouse) &&
				!CanChoose(Category.SmallStraight) && !CanChoose(Category.LargeStraight) &&
				!CanChoose(Category.Chance) && !CanChoose(Category.Yahtzee);
		}
	}
}