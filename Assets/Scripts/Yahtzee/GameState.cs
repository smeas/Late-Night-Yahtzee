using System;
using UnityEngine;

namespace Yahtzee {
	public enum PlayerIndex {
		PlayerOne = 0,
		PlayerTwo = 1,
	}

	[Serializable]
	public class GameState {
		[SerializeField] public PlayerIndex turn;
		[SerializeField] public PlayerState playerOne;
		[SerializeField] public PlayerState playerTwo;
	}
}