using System;
using UnityEngine;

namespace Yahtzee {
	public enum PlayerIndex {
		Player1,
		Player2,
	}

	[Serializable]
	public class GameState {
		[SerializeField] private PlayerIndex turn;
		[SerializeField] private PlayerState player1;
		[SerializeField] private PlayerState player2;
	}
}