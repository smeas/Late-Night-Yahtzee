using Cinemachine;
using UnityEngine;

public class SwitchCamera : MonoBehaviour {
	[SerializeField] private CinemachineVirtualCamera camera1;
	[SerializeField] private CinemachineVirtualCamera camera2;

	private CinemachineBrain brain;

	private void Start() {
		brain = GetComponent<CinemachineBrain>();
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			camera1.enabled = true;
			camera2.enabled = false;
		}

		if (Input.GetKeyDown(KeyCode.Alpha2)) {
			camera1.enabled = false;
			camera2.enabled = true;
		}
	}
}