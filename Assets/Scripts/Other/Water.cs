using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Based on:
// https://gamedev.stackexchange.com/questions/44547/how-do-i-create-2d-water-with-dynamic-waves

public class Water : MonoBehaviour {
	public int numPoints = 25;
	public float width = 10;

	public Transform splashSource;

	public float f1, f2, o1, o2, ts1, ts2, a;

	[Space]
	public float splashFreq;
	public float splashSpeed;

	[Space]
	public float springConstant;
	public float baselineSpringConstant;
	public float springDamping;

	private LineRenderer lineRenderer;
	private EdgeCollider2D edgeCollider;
	private Vector3[] points;
	private Vector2[] points2d;
	private SpringData[] springs;

	void Start() {
		lineRenderer = GetComponent<LineRenderer>();
		edgeCollider = GetComponent<EdgeCollider2D>();

		RebuildLine();
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha5)) {
			const int pts = 9;
			const float height = 2f;

			int start = (numPoints - pts) / 2;

			for (int i = 0; i < pts; i++) {
				float t = Mathf.Sin(i * Mathf.PI / pts);
				springs[start + i].y -= t * height;
			}
		}

		UpdateSprings();

		for (int i = 0; i < numPoints; i++) {
			points[i].y = points2d[i].y =
				springs[i].y +
				Mathf.Sin(f1 * points[i].x + o1 + ts1 * Time.time) *
				Mathf.Sin(f2 * points[i].x + o2 + ts2 * Time.time) * a;
		}

		lineRenderer.SetPositions(points);
		edgeCollider.points = points2d;
	}

	private void OnValidate() {
		if (lineRenderer == null) return;
		RebuildLine();
	}

	private void OnTriggerEnter2D(Collider2D other) {
		float minX = points[0].x;
		float maxX = points[points.Length - 1].x;
		float t = Mathf.InverseLerp(minX, maxX, other.transform.position.x);
		int index = (int)(points.Length * Mathf.Clamp01(t));

		//springs[index].speed -= 1f;

		AffectSprings(index, 4, other.attachedRigidbody.velocity.y * 0.01f);
	}

	// private void OnTriggerEnter2D(Collider2D other) {
	// 	var contacts = new ContactPoint2D[16];
	// 	int contactCount = other.GetContacts(contacts);
	//
	// 	print(contactCount);
	//
	// 	float minX = float.PositiveInfinity;
	// 	float maxX = float.NegativeInfinity;
	//
	// 	for (int i = 0; i < contactCount; i++) {
	// 		ContactPoint2D contact = contacts[i];
	// 		if (contact.collider != edgeCollider) continue;
	//
	// 		if (contact.point.x < minX)
	// 			minX = contact.point.x;
	//
	// 		if (contact.point.y > maxX)
	// 			maxX = contact.point.x;
	// 	}
	//
	// 	if (float.IsInfinity(minX) || float.IsInfinity(maxX))
	// 		return;
	//
	// 	float yVel = other.attachedRigidbody.velocity.y;
	//
	// 	for (int i = 0; i < numPoints; i++) {
	// 		if (points[i].x >= minX) {
	// 			springs[i].speed += yVel;
	// 		}
	//
	// 		if (points[i].x > maxX)
	// 			break;
	// 	}
	// }


	private void AffectSprings(int centerIndex, int radius, float amount) {
		int minIndex = Mathf.Max(centerIndex - radius, 0);
		int maxIndex = Mathf.Min(centerIndex + radius, points.Length - 1);
		int pointCount = radius * 2 + 1;

		for (int i = minIndex; i <= maxIndex; i++) {
			springs[i].speed += amount * Mathf.Sin(i * Mathf.PI / pointCount);
		}
	}

	private void RebuildLine() {
		lineRenderer.positionCount = numPoints;
		points = new Vector3[numPoints];
		points2d = new Vector2[numPoints];
		springs = new SpringData[numPoints];

		float step = width / numPoints;
		float x = -width / 2f + step / 2f;
		for (int i = 0; i < numPoints; i++) {
			points[i] = new Vector3(x, 0, 0);
			points2d[i] = new Vector2(x, 0);
			x += step;
		}

		lineRenderer.SetPositions(points);
		edgeCollider.points = points2d;
	}

	private void UpdateSprings() {
		for (int i = 0; i < numPoints; i++) {
			float y = springs[i].y;
			float leftY, rightY;

			if (i == 0)
				leftY = y;//springs[numPoints - 1].y;
			else
				leftY = springs[i - 1].y;

			if (i == numPoints - 1)
				rightY = y;//springs[0].y;
			else
				rightY = springs[i + 1].y;

			// Spring forces (F = kx).
			float leftForce = springConstant * (leftY - y);
			float rightForce = springConstant * (rightY - y);
			float baselineForce = baselineSpringConstant * -y;

			float force = leftForce + rightForce + baselineForce;
			float acceleration = force / 1f; // a = F/m

			springs[i].speed = springDamping * springs[i].speed + acceleration;
		}

		for (int i = 0; i < numPoints; i++) {
			springs[i].y += springs[i].speed;
		}
	}


	private struct SpringData {
		public float y;
		public float speed;
	}
}