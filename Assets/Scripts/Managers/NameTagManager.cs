using System;
using TMPro;
using UnityEngine;

public class NameTagManager : SingletonBehaviour<NameTagManager> {
	[SerializeField] private Canvas canvas;
	[SerializeField] private GameObject tagPrefab;
	[SerializeField] private Vector2 tagOffset;
	[SerializeField] private Transform[] players;

	private Transform[] nameTags;
	private Camera mainCamera;

	protected override void Awake() {
		base.Awake();

		mainCamera = Camera.main;

		Transform canvasRoot = new GameObject("Name Tags").transform;
		canvasRoot.parent = canvas.transform;

		nameTags = new Transform[players.Length];
		for (int i = 0; i < players.Length; i++) {
			nameTags[i] = Instantiate(tagPrefab).transform;
			nameTags[i].transform.SetParent(canvasRoot, false);
		}
	}

	private void LateUpdate() {
		for (int i = 0; i < players.Length; i++) {
			Vector2 worldPosition = (Vector2)players[i].transform.position + tagOffset;
			nameTags[i].position = mainCamera.WorldToScreenPoint(worldPosition);
		}
	}

	public void SetName(int playerIndex, string nameTag) {
		nameTags[playerIndex].GetComponentInChildren<TextMeshProUGUI>().text = nameTag;
	}

	public string GetName(int playerIndex) {
		return nameTags[playerIndex].GetComponentInChildren<TextMeshProUGUI>().text;
	}
}