using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI {
	public class MenuManager : MonoBehaviour {
		[SerializeField] private bool activateFirstMenuOnLoad = true;
		[SerializeField] private bool allowPoppingLastMenu = false;
		[SerializeField] private GameObject[] menus;

		private readonly Stack<int> stack = new Stack<int>();

		public int CurrentIndex => stack.Peek();
		public GameObject CurrentMenu => menus[CurrentIndex];
		public int OpenMenuCount => stack.Count;

		private void Awake() {
			if (menus.Length == 0) {
				Debug.LogWarning("MenuManager has no menus.");
				return;
			}

			// Don't deactivate the first menu if it's supposed to be active
			int startIndex = activateFirstMenuOnLoad ? 1 : 0;
			for (int i = startIndex; i < menus.Length; i++)
				menus[i].SetActive(false);

			if (activateFirstMenuOnLoad)
				PushMenu(0);
		}

		private void OnEnable() {
			if (stack.Count > 0)
				menus[stack.Peek()].SetActive(true);
		}

		private void OnDisable() {
			if (stack.Count > 0)
				menus[stack.Peek()].SetActive(false);
		}

		public void PushMenu(GameObject menu) {
			int index = Array.IndexOf(menus, menu);
			if (index == -1) {
				Debug.LogError("Menu not found.", menu);
				return;
			}

			PushMenu(index);
		}

		public void PushMenu(int menuIndex) {
			if (menuIndex < 0 || menuIndex >= menus.Length) {
				Debug.LogError("Menu index out of bounds.");
				return;
			}

			if (stack.Count > 0)
				menus[stack.Peek()].SetActive(false);
			menus[menuIndex].SetActive(enabled);
			stack.Push(menuIndex);
		}

		public void PopMenu() {
			if (stack.Count < 1 || stack.Count == 1 && !allowPoppingLastMenu)
				return;

			menus[stack.Pop()].SetActive(false);
			menus[stack.Peek()].SetActive(enabled);
		}
	}
}