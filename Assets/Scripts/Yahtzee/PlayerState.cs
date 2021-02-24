using System;
using UnityEngine;

namespace Yahtzee {
	// [Flags]
	// public enum Category {
	// 	Aces = 1 << 0,
	// 	Twos = 1 << 1,
	// 	Threes = 1 << 2,
	// 	Fours = 1 << 3,
	// 	Fives = 1 << 4,
	// 	Sixes = 1 << 5,
	// 	Bonus = 1 << 6,
	// 	Pair = 1 << 7,
	// 	TwoPair = 1 << 8,
	// 	ThreeOfAKind = 1 << 9,
	// 	FourOfAKind = 1 << 10,
	// 	FullHouse = 1 << 11,
	// 	SmallStraight = 1 << 12,
	// 	LargeStraight = 1 << 13,
	// 	Yahtzee = 1 << 14,
	// 	Chance = 1 << 15,
	// }

	[Serializable]
	public class PlayerState {
		// public int Aces { get; set; }
		// public int Twos { get; set; }
		// public int Threes { get; set; }
		// public int Fours { get; set; }
		// public int Fives { get; set; }
		// public int Sixes { get; set; }
		// public int Bonus { get; set; }
		//
		// public int Pair { get; set; }
		// public int TwoPair { get; set; }
		// public int ThreeOfAKind { get; set; }
		// public int FourOfAKind { get; set; }
		// public int FullHouse { get; set; }
		// public int SmallStraight { get; set; }
		// public int LargeStraight { get; set; }
		// public int Chance { get; set; }
		// public int Yahtzee { get; set; }

		[SerializeField]
		private int[] scores = new int[16];

		[SerializeField]
		private int scratchedMask;

		public int[] Scores => scores;
		public int UpperSum { get; private set; }
		public int TotalSum { get; private set; }

		// Bit-field with bit indices from `Category`.
		public int Scratched => scratchedMask;

		public int this[Category category] {
			get => scores[(int)category];
			set => scores[(int)category] = value;
		}

		// public int this[Category category] {
		// 	get {
		// 		return category switch {
		// 			Category.Aces => Aces,
		// 			Category.Twos => Twos,
		// 			Category.Threes => Threes,
		// 			Category.Fours => Fours,
		// 			Category.Fives => Fives,
		// 			Category.Sixes => Sixes,
		// 			Category.Bonus => Bonus,
		// 			Category.Pair => Pair,
		// 			Category.TwoPair => TwoPair,
		// 			Category.ThreeOfAKind => ThreeOfAKind,
		// 			Category.FourOfAKind => FourOfAKind,
		// 			Category.FullHouse => FullHouse,
		// 			Category.SmallStraight => SmallStraight,
		// 			Category.LargeStraight => LargeStraight,
		// 			Category.Yahtzee => Yahtzee,
		// 			Category.Chance => Chance,
		// 			_ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
		// 		};
		// 	}
		// 	set {
		// 		switch (category) {
		// 			case Category.Aces:
		// 				Aces = value;
		// 				break;
		// 			case Category.Twos:
		// 				Twos = value;
		// 				break;
		// 			case Category.Threes:
		// 				Threes = value;
		// 				break;
		// 			case Category.Fours:
		// 				Fours = value;
		// 				break;
		// 			case Category.Fives:
		// 				Fives = value;
		// 				break;
		// 			case Category.Sixes:
		// 				Sixes = value;
		// 				break;
		// 			case Category.Bonus:
		// 				Bonus = value;
		// 				break;
		// 			case Category.Pair:
		// 				Pair = value;
		// 				break;
		// 			case Category.TwoPair:
		// 				TwoPair = value;
		// 				break;
		// 			case Category.ThreeOfAKind:
		// 				ThreeOfAKind = value;
		// 				break;
		// 			case Category.FourOfAKind:
		// 				FourOfAKind = value;
		// 				break;
		// 			case Category.FullHouse:
		// 				FullHouse = value;
		// 				break;
		// 			case Category.SmallStraight:
		// 				SmallStraight = value;
		// 				break;
		// 			case Category.LargeStraight:
		// 				LargeStraight = value;
		// 				break;
		// 			case Category.Yahtzee:
		// 				Yahtzee = value;
		// 				break;
		// 			case Category.Chance:
		// 				Chance = value;
		// 				break;
		// 			default:
		// 				throw new ArgumentOutOfRangeException(nameof(category), category, null);
		// 		}
		// 	}
		// }

		public void Scratch(Category category) {
			scratchedMask |= 1 << (int)category;
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
			UpperSum = this[Category.Aces] + this[Category.Twos] + this[Category.Threes] +
				this[Category.Fours] + this[Category.Fives] + this[Category.Sixes];

			if (UpperSum >= 63 && !HasScratched(Category.Bonus))
				this[Category.Bonus] = 50;
			else
				this[Category.Bonus] = 0;

			TotalSum = UpperSum + this[Category.Bonus] + this[Category.Pair] + this[Category.TwoPair] +
				this[Category.ThreeOfAKind] + this[Category.FourOfAKind] + this[Category.FullHouse] +
				this[Category.SmallStraight] + this[Category.LargeStraight] + this[Category.Chance] +
				this[Category.Yahtzee];
		}
	}
}