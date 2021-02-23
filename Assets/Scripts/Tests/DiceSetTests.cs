using NUnit.Framework;
using Yahtzee;

namespace Tests {
	public class DiceSetTests {
		[TestCase(5, 5, 5, 1, 2, 5, ExpectedResult = 15)]
		[TestCase(1, 4, 6, 4, 1, 4, ExpectedResult = 8)]
		[TestCase(1, 1, 6, 4, 5, 3, ExpectedResult = 0)]
		public int GetNumberScore(int a, int b, int c, int d, int e, int value) {
			return new DiceSet(a, b, c, d, e).GetNumberScore(value);
		}

		[TestCase(1, 4, 6, 4, 1, 2, ExpectedResult = 8)]
		[TestCase(6, 6, 4, 4, 4, 2, ExpectedResult = 12)]
		[TestCase(6, 6, 4, 4, 4, 3, ExpectedResult = 12)]
		[TestCase(1, 2, 2, 2, 2, 4, ExpectedResult = 8)]
		[TestCase(1, 2, 3, 4, 5, 2, ExpectedResult = 0)]
		public int GetMultipleScore(int a, int b, int c, int d, int e, int count) {
			return new DiceSet(a, b, c, d, e).GetMultipleScore(count);
		}

		[TestCase(3, 3, 5, 6, 6, ExpectedResult = 18)]
		[TestCase(6, 6, 6, 6, 6, ExpectedResult = 0)]
		public int GetTwoPairScore(int a, int b, int c, int d, int e) {
			return new DiceSet(a, b, c, d, e).GetTwoPairScore();
		}

		[TestCase(5, 2, 5, 2, 2, ExpectedResult = 16)]
		[TestCase(6, 6, 1, 1, 6, ExpectedResult = 20)]
		[TestCase(6, 6, 6, 6, 6, ExpectedResult = 0)]
		public int GetFullHouseScore(int a, int b, int c, int d, int e) {
			return new DiceSet(a, b, c, d, e).GetFullHouseScore();
		}

		[TestCase(1, 2, 3, 4, 5, ExpectedResult = 15)]
		[TestCase(2, 5, 1, 4, 3, ExpectedResult = 15)]
		[TestCase(2, 3, 4, 5, 6, ExpectedResult = 0)]
		public int GetSmallStraightScore(int a, int b, int c, int d, int e) {
			return new DiceSet(a, b, c, d, e).GetSmallStraightScore();
		}

		[TestCase(2, 3, 4, 5, 6, ExpectedResult = 20)]
		[TestCase(4, 2, 6, 5, 3, ExpectedResult = 20)]
		[TestCase(1, 2, 3, 4, 5, ExpectedResult = 0)]
		public int GetLargeStraightScore(int a, int b, int c, int d, int e) {
			return new DiceSet(a, b, c, d, e).GetLargeStraightScore();
		}

		[TestCase(2, 2, 6, 4, 3, ExpectedResult = 17)]
		[TestCase(1, 1, 1, 1, 1, ExpectedResult = 5)]
		public int GetChanceScore(int a, int b, int c, int d, int e) {
			return new DiceSet(a, b, c, d, e).GetChanceScore();
		}

		[TestCase(1, 1, 1, 1, 1, ExpectedResult = 50)]
		[TestCase(5, 5, 5, 5, 5, ExpectedResult = 50)]
		[TestCase(3, 3, 1, 3, 3, ExpectedResult = 0)]
		public int GetYahtzeeScore(int a, int b, int c, int d, int e) {
			return new DiceSet(a, b, c, d, e).GetYahtzeeScore();
		}
	}
}