using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnHit : MonoBehaviour {
	private void OnTriggerEnter2D(Collider2D other) => OnEnter(other);
	private void OnCollisionEnter2D(Collision2D other) => OnEnter(other.collider);

	private void OnEnter(Collider2D other) {
		Destroy(other.gameObject);
	}
}