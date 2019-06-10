using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {

	[Header("GameObjects")]
	public Bullet bullet; //prefab for bullet instantiation 
	public Transform firePoint;// where to instantiate bullet.

	[Header("Configureables (Clip)")]
	public int startingClip; //size of the clip in the gun
	public float bulletSpeed;//bullet speed
	public int bulletRange; // range of bullet
	public int bulletDamage; // damage of the bullet
	public int bulletPenetration; // penetration level of the bullet
	public float fireRate; // fire rate
	public float reloadTime; // reload time

	[Header("Sound")]
	public AudioClip onFire;
	public AudioClip onReload;

	private TutorialManager tutorialManager;
	private SoundManager soundManager;
	private InputManager inputManager;
	private GameObject entity;
	private int currentClip;
	public int CurrentClip {
		get {
			return currentClip;
		}
	}

	private bool isReloading;
	public bool IsReloading {
		get {
			return isReloading;
		}
	}

	private bool isFiring;
	public bool IsFiring {
		get {
			return isFiring;
		}
		set {
			isFiring = value;
		}
	}
	private GameObject holder;

	/*
	Start of Ammo System, if desired:
	[Header("Configureables (Ammo)")]
	public bool usesAmmo;
	public int startingAmmo;
	private int currentAmmo;
	*/
 
	private float shotCounter;// timer working behind fire rate

	Ray ray;
	RaycastHit hit;

	void Start () {
		isReloading = false;
		currentClip = startingClip;
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
		tutorialManager = GameObject.Find ("GameManager").GetComponent<TutorialManager> ();
		inputManager = GameObject.Find ("GameManager").GetComponent<InputManager> ();
	}

	// Update is called once per frame
	void Update () {
		if (gameObject.tag == "Untagged") {
			// It's being held, so let's see by whom
			if (holder == null) {
				holder = gameObject.transform.parent.parent.gameObject;
			}

			// Decrease shotCounter (time between each shot)
			if (shotCounter > 0) {
				shotCounter -= Time.deltaTime;
			}

			// Called if the player is shooting
			if (isFiring) {
				FireGun (false);
			}

			// User presses 'R' key to reload
			if (inputManager.InputMatches ("WEAPON_RELOAD")) {
				StartReload ();
			}
		} else {
			isReloading = false;
			StopCoroutine ("Reload");
		}
	}

	public void SetEntity(GameObject newEntity) {
		entity = newEntity;
	}

	public void StartReload() {
		if (!isReloading && currentClip != startingClip) {
			StartCoroutine ("Reload");
		}
	}

	public void FireGun(bool ignoreAmmo) {
		if (shotCounter <= 0) {
			if (currentClip > 0|| ignoreAmmo) {
				// Stop reloading (prevents infinite-ish ammo)
				if (isReloading) {
					StopReloading ();
				}

				// Play the sound
				if (onFire != null) {
					soundManager.InstantiateSound ("Gunshot", gameObject, onFire, 120, false, false, true);
				}

				// Reset the shotCounter (delay until next bullet)
				shotCounter = fireRate;

				// Spawn a new bullet, setting its variables
				Bullet newBullet = Instantiate (bullet, firePoint.position, firePoint.rotation) as Bullet;
				newBullet.SetVelocity (bulletSpeed);
				newBullet.SetRange (bulletRange);
				newBullet.SetDamage (bulletDamage);
				newBullet.SetPenetration (bulletPenetration);
				newBullet.SetShooter (entity);
				newBullet.name = bullet.name;

				// Reduce the current clip, if doesn't ignore ammo
				if (!ignoreAmmo) {
					currentClip--;
				}
			}
		}

		// Check to see if the current clip is empty (and player is firing): reload
		if (currentClip <= 0) {
			isFiring = false;
			StartReload ();
		}
	}

	IEnumerator Reload() {
		// Show the tutorial
		if (tutorialManager != null) {
			//tutorialManager.ShowTutorial ("TUTORIAL_6");
		}

		// Play the sound
		if (onReload != null && soundManager != null) {
			soundManager.InstantiateSound ("Reload", gameObject, onReload, 120, false, true, true);
		}

		// Reload the gun
		isReloading = true;

		// Wait for the time to pass
		yield return new WaitForSeconds (reloadTime);

		// Fill the gun with new ammo
		currentClip = startingClip;
		isReloading = false;
	}

	// Cancels reload, for whatever reason (ie. user shoots again)
	public void StopReloading() {
		isReloading = false;

		// Stop the sound effect
		if (soundManager != null) {
			soundManager.StopInstantiatedSound ("Reload", gameObject);
		}

		StopCoroutine ("Reload");
	}
}