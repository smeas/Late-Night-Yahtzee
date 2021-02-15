using UnityEngine;

public class PlayerInput : MonoBehaviour {
	[SerializeField] private InputType inputType;

	private PlayerController playerController;
	private Camera mainCamera;

	private void Start() {
		playerController = GetComponent<PlayerController>();
		mainCamera = Camera.main;
	}

	private void Update() {
		switch (inputType) {
			case InputType.Keyboard:
				Vector2 movementDelta = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
				playerController.Move(movementDelta.normalized);
				break;

			case InputType.Mouse:
				if (Input.GetMouseButton(1)) {
					Vector2 targetPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
					playerController.MoveTowardsPoint(targetPoint);
				}
				break;
		}
	}


	private enum InputType {
		Keyboard,
		Mouse,
	}
}