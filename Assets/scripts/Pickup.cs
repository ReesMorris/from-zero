using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

	public Pickups.Type pickupType;
	public Pickups.Slot pickupSlot;
	public Pickups.DropDirection dropDirection;
	[Range(0, 100)]
	public int rarity;
	public Vector3 defaultRotation;

	public void Use() {
		switch (pickupType) {
		case Pickups.Type.Gun:
			GetComponent<Gun> ().FireGun (false);
			break;
		case Pickups.Type.Health:
			GetComponent<HealthItem> ().StartHealing ();
			break;
		}
	}

	public void StopUse() {
		switch (pickupType) {
		case Pickups.Type.Gun:
			GetComponent<Gun> ().StopReloading ();
			break;
		case Pickups.Type.Health:
			GetComponent<HealthItem> ().StopHealing ();
			break;
		}
	}
}
