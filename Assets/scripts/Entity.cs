using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class Entity : MonoBehaviour {

	[Header("Current Item")]
	public Pickup holding;

	[Header("Health")]
	public bool invincible = false;
	public float maxHealth = 100;
	public float startingHealth = 100;
	private bool isHealing = false;
	public bool despawnOnDeath = true;
	public bool IsHealing {
		get {
			return isHealing;
		}
		set {
			isHealing = value;
		}
	}

	[Header("Health Bar")]
	public bool showHealthBarAlways = false;
	public HealthBar healthBar;
	private float currentHealth;
	public float CurrentHealth {
		get {
			return currentHealth;
		}
	}
	private bool isDead;
	public bool IsDead {
		get {
			return isDead;
		}
	}

	[Header("Looking")]
	public bool lookAtObject;
	public GameObject lookObject;

	[Header("Sound")]
	public AudioClip onHit;
	public AudioClip onDeath;
	public AudioClip noise;

	private SoundManager soundManager;
	private Rigidbody rb;
	private NavMeshAgent navMesh;
	private PlayerStats playerStats;
	private Animator animator;

	void Start () {
		isDead = false;

		// Hide the health bar, unless we always are showing it
		if (healthBar != null) {
			healthBar.showAlways = showHealthBarAlways;
		}

		currentHealth = startingHealth;
		rb = GetComponent<Rigidbody> ();
		navMesh = GetComponent<NavMeshAgent> ();
		animator = GetComponent<Animator> ();

		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
		playerStats = GameObject.Find ("GameManager").GetComponent<PlayerStats> ();

		SetupAudio ();
	}

	void Update() {
		LookAt ();
	}

	void LookAt() {
		if (lookAtObject && lookObject != null) {
			Vector3 look = new Vector3 (lookObject.transform.position.x, this.transform.position.y, lookObject.transform.position.z);
			transform.LookAt (look);
		}
	}

	// Set up the audio for the entity
	void SetupAudio() {
		if (noise != null) {
			soundManager.InstantiateSound ("Noise", gameObject, noise, 60, true, false, true);
		}
	}

	// Returns true if the entity has space to heal
	public bool CanHeal() {
		if (currentHealth >= maxHealth) {
			return false;
		}
		return true;
	}

	public void SetLookAt(GameObject item) {
		lookObject = item;
		lookAtObject = true;
	}

	public void StopLookAt() {
		lookObject = null;
		lookAtObject = false;
	}

	public void IncreaseHealth(float amount) {
		currentHealth += amount;

		// Prevent the health from exceeding the maximum health
		if (currentHealth > maxHealth) {
			currentHealth = maxHealth;
		}
	}

	public void ReduceHealth(float amount) {
		if (!invincible) {
			currentHealth -= amount;

			// Dead?
			if (currentHealth <= 0) {
				OnDeath ();
			}

			// Play a damage sound
			if (onHit != null) {
				soundManager.InstantiateSound("Zombie", gameObject, onHit, 120, false, false, true);
			}

			// Health bar?
			if (healthBar != null) {
				// Convert to a float, since we are working with values between 0 and 1
				float newHealth = (float) currentHealth / (float) startingHealth;

				// Change the health bar amount
				healthBar.SetFillAmount(newHealth);
			}
		}
	}

	// Called when a collision takes place
	void OnCollisionEnter(Collision collision) {
		// Take damage
		if (collision.gameObject.tag == "Zombie") {
			if (gameObject.tag != "Zombie") {
				ReduceHealth (collision.gameObject.GetComponent<Zombie> ().damageDealt);
			}
		}
	}

	void OnDeath() {
		isDead = true;

		if (gameObject.tag == "Zombie") {
			playerStats.zombieKills++;
		}

		// Destroy healthbar
		if (healthBar != null) {
			healthBar.Destroy();
		}

		navMesh.enabled = false;
		gameObject.tag = "Untagged";
		gameObject.layer = 11;
		if (GetComponent<Animator> ()) {
			GetComponent<Animator> ().enabled = false;
		}

		if (rb.velocity == Vector3.zero) {
			rb.velocity = new Vector3 (0f, 0f, 100f);
		}
		rb.AddForce (Vector3.forward);

		// Stop anims
		if (animator != null) {
			animator.StopPlayback ();
		}

		if (gameObject.name != "Player") {
			if (despawnOnDeath) {
				StartCoroutine ("TriggerDeathSequence");
			}
		} else {
			// Call the OnDeath function, handled by the Player.cs script
			GetComponent<Player> ().OnDeath ();
		}
	}

	// Change the max health of the entity
	public void SetMaxHealth(float newHealth) {
		maxHealth = newHealth;
		startingHealth = maxHealth;
		currentHealth = maxHealth;
	}

	IEnumerator TriggerDeathSequence() {
		yield return new WaitForSeconds (3f);
		Destroy (gameObject);
	}

	// Called when the mouse is over the entity
	void OnMouseOver()
	{
		// Show the health bar, if it exists
		if (healthBar != null) {
			healthBar.Show();
		}
	}

	// Called when the mouse is no longer over the entity
	void OnMouseExit()
	{
		// Hide the health bar, if it exists
		if (healthBar != null && !showHealthBarAlways) {
			healthBar.Hide();
		}
	}
}
