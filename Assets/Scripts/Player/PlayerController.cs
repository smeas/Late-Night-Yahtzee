using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour {
	[SerializeField] private float speed = 5f;

	private Rigidbody2D rb2d;

	private void Awake() {
		rb2d = GetComponent<Rigidbody2D>();
	}

	public void Move(Vector2 delta) {
		MoveDirect(delta * (speed * Time.deltaTime));
	}

	public void MoveTowardsPoint(Vector2 point) {
		Vector2 direction = point - (Vector2)transform.position;
		float squareDirection = direction.sqrMagnitude;
		if (squareDirection == 0)
			return;

		float distance = Mathf.Sqrt(squareDirection);
		direction *= 1f / distance; // Normalize

		float step = speed * Time.deltaTime;
		step = Mathf.Min(step, distance); // Make sure we don't pass the point

		MoveDirect(direction * step);
	}

	private void MoveDirect(Vector2 delta) {
		rb2d.MovePosition(transform.position + (Vector3)delta);
	}
}