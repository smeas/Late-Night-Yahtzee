using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSpawner : MonoBehaviour {
	[SerializeField] private GameObject prefab;

	private Camera mainCamera;

	void Start() {
		mainCamera = Camera.main;
	}

	private void Update() {
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(1)) {
			Vector2 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
			Instantiate(prefab, pos, Quaternion.identity);
		}
	}
}