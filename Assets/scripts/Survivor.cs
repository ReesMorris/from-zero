using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class Survivor : MonoBehaviour {

	public enum Type { Friendly, Hostile };
	public Type type;

	[Header("Config")]
	public bool shootAtEnemies;
	public bool moveAwayFromEnemies;

	[Header("Variables")]
	public float searchRadius;
	public float shootRadius;
	public float moveAwayDistance;

	private Entity entity;
	private Gun gun;
	private GameObject target;
	private GameObject escape;
	private Pathfinding pathfinding;
	private Inventory inventory;

	void Start() {
		entity = GetComponent<Entity>();
		pathfinding = GetComponent<Pathfinding> ();
		inventory = GetComponent<Inventory> ();
		escape = new GameObject ();
		escape.transform.SetParent (transform);

		DebugErrors ();
	}

	void Update() {
		UpdateTarget ();
		HandleShooting ();
	}

	// Helpful incase the values are wrong
	void DebugErrors() {
		if (searchRadius < shootRadius) {
			Debug.LogError (this.name + ": searchRadius should not be less than shootRadius");
		}
		if (moveAwayDistance > shootRadius) {
			Debug.LogError (this.name + ": moveAwayDistance should not be more than shootRadius");
		}
	}

	void ShootAtTarget() {
		if (shootAtEnemies) {
			if (entity.holding != null) {
				transform.LookAt (target.transform.position);
				entity.holding.Use ();
			}
		}
	}

	void UpdateTarget() {
		target = FindPotentialTargets ();
	}

	// Will approach a target if he is too far, otherwise will just shoot
	void HandleShooting() {
		SwitchWeapon ();
		if (target != null) {
			float distance = Vector3.Distance (transform.position, target.transform.position);
			if (distance > shootRadius) {
				if (shootAtEnemies) {
					pathfinding.target = target;
				}
			} else {
				ShootAtTarget ();
				MoveAwayFromApproachingTarget ();
			}
		} else {
			StopMovingFromApproachingTarget ();
		}
	}

	// Switch to secondary to save time
	void SwitchWeapon() {
		SetGun ();

		// No target, might as well reload our primary weapon
		if (target == null && !gun.IsReloading) {
			inventory.EquipNextAvailable ();
			SetGun ();
			gun.StartReload ();
		}

		// We're unarmed right now, switch to secondary if possible
		if (gun == null) {
			SetGun ();
		}

		if (gun.IsReloading && target != null && inventory.CurrentSlot ().slotType == Pickups.Slot.Primary) {
			inventory.EquipInventorySlot (Pickups.Slot.Secondary);
		}
	}

	void SetGun() {
		gun = entity.holding.GetComponent<Gun>();
	}

	void MoveAwayFromApproachingTarget() {
		if (moveAwayFromEnemies) {
			float distance = Vector3.Distance (transform.position, target.transform.position);
			// Only move if the entity is close enough
			if (distance < moveAwayDistance) {
				// src: https://forum.unity.com/threads/move-away-from-object.57729/#post-368996
				Vector3 direction = transform.position - target.transform.position;
				direction.Normalize ();

				escape.transform.position = transform.position + direction + direction;
				pathfinding.target = escape;
			} else {
				StopMovingFromApproachingTarget ();
			}
		}
	}

	void StopMovingFromApproachingTarget() {
		if (moveAwayFromEnemies) {
			pathfinding.target = null;
		}
	}

	// Will find all potential targets, and then the closest one if possible
	GameObject FindPotentialTargets() {
		List<GameObject> targets = new List<GameObject>();

		// src : https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html 11 feb 2018
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, searchRadius);
		for (int i = 0; i < hitColliders.Length; i++) {
			GameObject hit = hitColliders [i].gameObject;
			Entity hitEntity = hit.GetComponent<Entity> ();
			Survivor hitSurvivor = hit.GetComponent<Survivor> ();

			// Narrow it down to entities only
			if (hitEntity != null) {
				// Now, let's exclude ourself
				if (hit.name != gameObject.name) {
					// Now, let's make sure it's only entities that are the opposite of us (hostile, friendly) or zombies

					// Is it a zombie?
					if (hit.tag == "Zombie") {
						targets.Add (hit);
					}
					// Is it the player, and are we hostile?
					else if (hit.name == "Player" && type == Type.Hostile) {
						targets.Add (hit);
					}
					// It must be another survivor, are they the oppsoite to us?
					else if (hitSurvivor != null) {
						if (hitSurvivor.type != type) {
							targets.Add (hit);
						}
					}
				}
			}
		}

		// find the closest target we can see
		return FindClosestTarget (targets);
	}

	GameObject FindClosestTarget(List<GameObject> targets) {
		Vector3 originalRot = transform.eulerAngles;
		float closestDistance = float.MaxValue;
		GameObject closestObject = null;

		for (int i = 0; i < targets.Count; i++) {
			GameObject target = targets [i];

			// If it's not dead by this frame
			if (!target.GetComponent<Entity> ().IsDead) {
				// Compute the distance
				float distance = Vector3.Distance (transform.position, target.transform.position);
				if (distance < closestDistance) {
					// The entity is closer than the previous, but can we actually see it?
					transform.LookAt (target.transform.position);
					RaycastHit hit;
					if (Physics.Raycast (transform.position, transform.forward, out hit, 100f)) {
						if (hit.transform.gameObject.name == target.name) {
							// Clear line of sight, let's update our values
							closestDistance = distance;
							closestObject = target;
						}
					}
				}
			}
		}

		// Reset our rotation so it looks like we've never even moved
		transform.eulerAngles = originalRot;

		return closestObject;
	}
}
