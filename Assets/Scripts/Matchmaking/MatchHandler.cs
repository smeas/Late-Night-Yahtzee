using System;
using Firebase.Database;
using Managers;
using UnityEngine;

namespace Matchmaking {
	public class MatchHandler {
		public string MatchId { get; }
		public DatabaseReference Reference { get; }
		public bool IsMatchDeleted { get; private set; }

		public event Action<GameInfo> Start;
		public event Action<UserInfo> PlayerJoined;
		public event Action<string> PlayerLeft;
		public event Action MatchDeleted;

		private readonly DatabaseReference playersReference;
		private readonly DatabaseReference startedReference;

		private bool enabled;

		public MatchHandler(string id) {
			MatchId = id;
			Reference = MatchmakingManager.MatchesReference.Child(id);
			playersReference = Reference.Child(nameof(MatchInfo.players));
			startedReference = Reference.Child(nameof(MatchInfo.started));
		}

		public void Enable() {
			if (enabled) return;
			enabled = true;

			playersReference.ChildAdded += OnPlayerAdded;
			playersReference.ChildRemoved += OnPlayerRemoved;
			startedReference.ValueChanged += OnStartedChanged;
			// The only time children of this node (MatchInfo) gets removed is if the whole match is removed.
			Reference.ChildRemoved += OnMatchDeleted;
		}

		public void Disable() {
			if (!enabled) return;
			enabled = false;

			playersReference.ChildAdded -= OnPlayerAdded;
			playersReference.ChildRemoved -= OnPlayerRemoved;
			startedReference.ValueChanged -= OnStartedChanged;
			Reference.ChildRemoved -= OnMatchDeleted;
		}

		private async void OnPlayerAdded(object sender, ChildChangedEventArgs e) {
			string id = e.Snapshot.Value as string;
			if (id == null) {
				Debug.Assert(false);
				return;
			}

			// FIXME(RC): Fetching the user info delays dispatching of the event. If a player joins and then immediately
			// the game is started, the join event might be missed because it got dispatched after the started event.
			UserInfo userInfo = await FirebaseManager.Instance.GetUserInfo(id);
			if (userInfo == null)
				Debug.Assert(false);

			PlayerJoined?.Invoke(userInfo);
		}

		private void OnPlayerRemoved(object sender, ChildChangedEventArgs e) {
			if (IsMatchDeleted)
				return;

			string id = e.Snapshot.Value as string;
			if (id == null) {
				Debug.Assert(false);
				return;
			}

			PlayerLeft?.Invoke(id);
		}

		private async void OnStartedChanged(object sender, ValueChangedEventArgs e) {
			if (IsMatchDeleted)
				return;

			bool? value = e.Snapshot.Value as bool?;
			if (value == true) {
				GameInfo gameInfo = await MatchmakingManager.GetGame(MatchId);
				Debug.Assert(gameInfo != null, "gameInfo != null");

				Start?.Invoke(gameInfo);
			}
		}

		private void OnMatchDeleted(object sender, ChildChangedEventArgs e) {
			IsMatchDeleted = true;

			MatchDeleted?.Invoke();
		}
	}
}