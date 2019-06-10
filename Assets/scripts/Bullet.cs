using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour {
	private int range; // range of the bullet before it is destroyed
	private float velocity; // speed of the bullet
	private int damage; // damage of the bullet
	private int distanceTravelled; // counts how far the bullet has travelled
	private int penetration; // amount of enemies that can be shot before bullet breaks
	private GameObject shooter; // the gameobject of the entity that shot the bullet
	private Rigidbody rb;
	private bool canMove;

	void Start () {
		canMove = true;
		rb = GetComponent<Rigidbody> ();
		rb.useGravity = false;

		if (shooter != null) {
			if (shooter.name == "Player") {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, Mathf.Infinity, LayerMask.GetMask ("Floor"))) {
					// Debug
					//Debug.DrawLine (ray.origin, hit.point);
					//Debug.DrawLine (hit.point, new Vector3(hit.point.x, 100f, hit.point.y), Color.green);

					// Player can't shoot towards themselves
					if (Vector3.Distance (hit.point, shooter.transform.position) > 1.7f) {
						transform.LookAt (new Vector3 (hit.point.x, shooter.transform.position.y, hit.point.z));
					}
				}
			}
		}
	}

	void Update () {
		// Move the bullet
		if (canMove) {
			rb.velocity = transform.forward * velocity * 50f * Time.deltaTime;

			// Increase distance travelled
			distanceTravelled++;

			// Destroy if travelled further than bullet range
			if (distanceTravelled >= range) {
				Destroy (gameObject);
			}
		}
	}

	// Used to set the velocity of the bullet
	public void SetVelocity(float newVelocity) {
		velocity = newVelocity;
	}

	// Used to set the range of the bullet
	public void SetRange(int newRange) {
		range = newRange;
	}

	// Used to set the damage of the bullet
	public void SetDamage(int newDamage) {
		damage = newDamage;
	}

	// Used to get the damage of the bullet
	public int GetDamage() {
		return damage;
	}

	// Used to set the penetration of the bullet
	public void SetPenetration(int newPenetration) {
		penetration = newPenetration;
	}

	// Reduce penetration level
	public void ReducePenetration(int amount) {
		penetration -= amount;
		if (penetration <= 0) {
			Destroy (gameObject);
		}
	}

	// Used to set the shooter of the bullet
	public void SetShooter(GameObject newShooter) {
		shooter = newShooter;
	}

	// Used to set the shooter of the bullet
	public GameObject GetShooter() {
		return shooter;
	}

	// Called when bullet collides with a gameobject
	void OnCollisionEnter(Collision collision) {
		Entity entity = collision.gameObject.GetComponent<Entity> ();

		// Check if collision is with an entity
		if (entity != null) {
			// Prevents players from shooting themselves
			if (shooter != collision.gameObject) {
				// Reduce health of entity and reduce bullet penetration value
				entity.ReduceHealth (damage);
				ReducePenetration (1);
			} else {
				// Bullet collided with own self, let's disable it
				DisableBullet();
			}
		} else {
			// Bullet collided with something solid; let's add some nice physics
			DisableBullet();
		}
	}

	// Called when a bullet collides with a trigger
	void OnTriggerEnter(Collider collider) {
		PenetrableSurface surface = collider.gameObject.GetComponent<PenetrableSurface> ();

		// Was this a surface that we can shoot through?
		if (surface != null) {
			ReducePenetration (surface.penetrationReduction);
		}
	}

	// Keeps bullet in the scene, but makes it do nothing
	void DisableBullet() {
		canMove = false;
		rb.useGravity = true;
		//rb.velocity = Vector3.zero;
		gameObject.layer = 10;
		StartCoroutine (DestroyBullet (5));
	}

	public IEnumerator DestroyBullet(float delay) {
		yield return new WaitForSeconds (delay);
		Destroy (gameObject);
	}
}