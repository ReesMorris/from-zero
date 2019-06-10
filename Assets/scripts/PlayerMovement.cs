using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FOR FROM-ZERO CREW:
// IN ORDER TO USE THE ROTATION FUNCTION 
// YOU NEED TO ADD A LAYER CALLED "floor" 
// TO YOUR SCENE GROUND. THANKS

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {

	[Header("Movement")]
	public float movementForceWalk = 400f; // initial speed
	public float movementForceSprint = 500f; // initial speed
	public float sprintSpeed = 1f; // initial boost (when the user sprints)
	public float sprintDuration = 50f;
	public float maxVelocityWalk = 2f;
	public float maxVelocitySprint = 4f;

	[Header("Sound")]
	public AudioClip walking;

	private Rigidbody rb;
	private float storeInitDuration;
	private float raycastRange;
	private int floorMask;
	private bool moving;
	private bool sprinting;
	private float currentSpeed;
	private CutsceneManager cutsceneManager;
	private InputManager inputManager;
	private Entity entity;
	private InventorySearcher inventorySearcher;
	private ConsoleManager consoleManager;
	private SoundManager soundManager;
	private UIManager uiManager;
	private Animator animator;
	private bool walkingAnim;

	// Init
	void Start () {
		rb = GetComponent<Rigidbody> ();
		entity = GetComponent<Entity> ();
		currentSpeed = movementForceWalk;
		storeInitDuration = sprintDuration;
		raycastRange = 100f;
		floorMask = LayerMask.GetMask ("Floor");
		cutsceneManager = GameObject.Find ("GameManager").GetComponent<CutsceneManager> ();
		inputManager = GameObject.Find ("GameManager").GetComponent<InputManager> ();
		consoleManager = GameObject.Find ("GameManager").GetComponent<ConsoleManager> ();
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
		uiManager = GameObject.Find ("GameManager").GetComponent<UIManager> ();
		inventorySearcher = GetComponent<InventorySearcher> ();
		animator = GetComponent<Animator> ();
	}
	
	// Update
	void Update () {
		if (!cutsceneManager.CutsceneActive) {
			if (Input.GetKey (KeyCode.LeftShift)) {
				sprintDuration--;
				if (sprintDuration <= 0) {
					sprintDuration = 0;
				}
			} else {
				sprintDuration++;
				if (sprintDuration >= storeInitDuration) {
					sprintDuration = storeInitDuration;
				}
			}

			if (!consoleManager.ConsoleOpen) {
				Sprint ();
				Movement ();
				Rotation ();
				Animate ();
			}
		} else {
			moving = false;
			sprinting = false;
			rb.velocity = Vector3.zero;
			rb.isKinematic = true;
			rb.rotation = rb.rotation;
		}
	}

	void Movement () {
		// translate the player position using W,A,S,D inputs -- [2]
		moving = false;

		// CONTROL SCHEME A (USES THE CAMERA AS FORWARD)
		if(PlayerPrefs.GetInt ("movement_control_scheme", 0) == 0) {
			if (inputManager.InputMatches("MOVE_FORWARD")) {
				rb.velocity += (-Vector3.right * currentSpeed * Time.deltaTime);
				moving = true;
			}
			if (inputManager.InputMatches("MOVE_LEFT")) {
				rb.velocity += (-Vector3.forward * currentSpeed * Time.deltaTime);
				moving = true;
			}
			if (inputManager.InputMatches("MOVE_RIGHT")) {
				rb.velocity += (Vector3.forward * currentSpeed * Time.deltaTime);
				moving = true;
			}
			if (inputManager.InputMatches("MOVE_BACKWARD")) {
				rb.velocity += (Vector3.right * currentSpeed * Time.deltaTime);
				moving = true;
			}
		} 

		// CONTROL SCHEME B (BETA METHOD, USES THE MOUSE AS FORWARD)
		else {
			if (inputManager.InputMatches("MOVE_FORWARD")) {
				rb.velocity += (transform.forward * currentSpeed * Time.deltaTime);
				moving = true;
			}
			if (inputManager.InputMatches("MOVE_LEFT")) {
				rb.velocity += (-transform.right * currentSpeed * Time.deltaTime);
				moving = true;
			}
			if (inputManager.InputMatches("MOVE_RIGHT")) {
				rb.velocity += (transform.right * currentSpeed * Time.deltaTime);
				moving = true;
			}
			if (inputManager.InputMatches("MOVE_BACKWARD")) {
				rb.velocity += (-transform.forward * currentSpeed * Time.deltaTime);
				moving = true;
			}
		}

		// Limit the maximum spoeed of the player [6]
		if (!sprinting) {
			if (rb.velocity.magnitude > maxVelocityWalk) {
				rb.velocity = Vector3.ClampMagnitude (rb.velocity, maxVelocityWalk);
			}
		} else {
			if (rb.velocity.magnitude > maxVelocitySprint) {
				rb.velocity = Vector3.ClampMagnitude (rb.velocity, maxVelocitySprint);
			}
		}

		if (!moving) {
			rb.velocity = Vector3.zero;

			// Kinematic prevents the player from being able to slide around whilst not moving [7]
			rb.isKinematic = true;
		} else {
			uiManager.HideReadable ();
			if (walking != null) {
				soundManager.InstantiateSound ("walking", gameObject, walking, 120, true, true, true);
			}
			rb.isKinematic = false;
			inventorySearcher.SmartSearch = true;
		}
	}

	void Rotation () {
		// creating a Ray that goes from the camera to the mouse position
		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit; // here we store the information when the ray hits something

		// if the Raycast function returns true we use those information
		// ray is the ray that we are casting that goes through the camera
		// out hit means that the returning info will be stored in hit
		// raycastRange is a float that represent the ray range
		// floorMask defines what kind of object the ray needs to hit to get info back
		// [2]
		if (Physics.Raycast (ray, out hit, raycastRange, floorMask)) {
			Vector3 playerToMouse = hit.point - transform.position;
			playerToMouse.y = 0f; // player's 'y' needs to be fixed

			// making a quaternion that will represent the angle needed
			Quaternion newRotation = Quaternion.LookRotation (playerToMouse);
			rb.MoveRotation (newRotation);
		}
	}

	// first sprinting function, may be a stratch
	void Sprint() {
		if (!entity.IsHealing) {
			// left shift for sprint
			if (inputManager.InputMatches ("MOVE_SPRINT") && sprintDuration > 0) {
				StartSprint ();
			} 

			// when left shift is released -> normal speed
			if (!inputManager.InputMatches ("MOVE_SPRINT") || sprintDuration <= 0) {
				StopSprint ();
			}
		} else {
			// Just to ensure that the player isn't sprinting already, let's stop them
			StopSprint();
		}
	}

	// Animate the player walking
	void Animate() {
		animator.speed = 1f;
		if (sprinting) {
			animator.speed = 1.9f;
		}
		if (moving && !walkingAnim) {
			animator.Play ("player_stand_to_walk");
			walkingAnim = true;
		}
		if (!moving && walkingAnim) {
			animator.Play ("player_walk");
			walkingAnim = false;
		}
		if (!moving && !walkingAnim) {
			animator.Play ("player_stand");
		}
	}

	// Trigger the entity to start sprinting
	void StartSprint() {
		currentSpeed = movementForceSprint;
		sprinting = true;
	}

	// Trigger the entity to stop sprinting
	void StopSprint() {
		currentSpeed = movementForceWalk;
		sprinting = false;
	}
}
