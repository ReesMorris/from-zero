using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DatabasePickups : MonoBehaviour {

	public List<Pickup> pickups;

	public GameObject GetPickup(string name) {
		for (int i = 0; i < pickups.Count; i++) {
			if (pickups [i] != null) { // failsafe
				if (name.ToLower () == pickups [i].name.ToLower ()) {
					return pickups [i].gameObject;
				}
			}
		}
		return null;
	}

	public GameObject GetPickupByID(int index) {
		if(index < pickups.Count) {
			return pickups [index].gameObject;
		}
		return null;
	}
}
