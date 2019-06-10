using UnityEngine;

public class CameraController : MonoBehaviour {

	[Header("GameObjects")]
	public GameObject mainCamera;
	public GameObject player;

	[Header("Configurables")]
	public int distanceX = 10;
	public int distanceY = 10;
	[Space(10)]
	public bool canZoom = true;
	public bool invertFovZoom = false;
	public int fovInit = 50;
	public int fovSpeed = 3;
	public int fovMin = 40;
	public int fovMax = 65;
	[Space(10)]
	public int rotateSpeed = 2;

	private Camera cameraComponent;
	private float fieldOfView;
	private InputManager inputManager;
	private ConsoleManager consoleManager;

    // Initialise the program
	void Start () {
		inputManager = GameObject.Find ("GameManager").GetComponent<InputManager> ();
		consoleManager = GameObject.Find ("GameManager").GetComponent<ConsoleManager> ();

		InitZoom ();
		InitPosition ();
	}

    // Called every frame, so the player is still able to zoom and rotate
	void Update () {
		Zoom ();
		SetPosition ();
	}

	// Set the initial position for the camera
	void InitPosition() {
		SetPosition ();

		// Camera to look at player
		mainCamera.transform.LookAt (player.transform);
	}

    // Predefines some necessary zoom variables
	void InitZoom() {
		cameraComponent = mainCamera.GetComponent<Camera> ();
		SetFov (fovInit);
		fieldOfView = cameraComponent.fieldOfView;
	}

    // Sets the position of the camera on start
	void SetPosition() {
		if (player != null) {
			Vector3 offset = new Vector3 (distanceX, distanceY, 0);
			mainCamera.transform.position = player.transform.position + offset;
		}
	}

    // Allows the player to zoom the camera in/out
	void Zoom() { // [1]
		if (canZoom && !consoleManager.ConsoleOpen) {
			
			// Zooming in
			if (inputManager.InputMatches("CAMERA_ZOOM_IN")) {
				// Decrease the camera FOV, if possible
				fieldOfView -= fovSpeed;
				if (fieldOfView < fovMin) {
					fieldOfView = fovMin;
				}
				SetFov (fieldOfView);
			}
			// Zooming out
			else if (inputManager.InputMatches("CAMERA_ZOOM_OUT")) {
				// Increase the camera FOV, if possible
				fieldOfView += fovSpeed;
				if (fieldOfView > fovMax) {
					fieldOfView = fovMax;
				}
				SetFov (fieldOfView);
			}
		}
	}

	// Used to set the FOV of the camera
	void SetFov(float newFov) {
		cameraComponent.fieldOfView = newFov;
	}
}
