using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Inventory : MonoBehaviour {

	public Pickup[] startingItems;
	public GameObject weaponHolder;

	private int currentEquippedSlot;
	private List<InventorySlot> inventoryItems;
	private SoundManager soundManager;
	private Vector3 initialWeaponHolderRot;
	private DatabasePickups databasePickups;
	private Animator animator;

	// Use this for initialization
	void Start () {
		initialWeaponHolderRot = weaponHolder.transform.localEulerAngles;
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
		databasePickups = GameObject.Find ("Database").transform.Find ("Pickups").GetComponent<DatabasePickups> ();
		animator = GetComponent<Animator> ();

		DefaultSlots ();
		AddStartingItems ();
	}
		
	// Set up the inventory back to its default slots
	void DefaultSlots() {
		currentEquippedSlot = -1;
		inventoryItems = new List<InventorySlot> ();

		inventoryItems.Add (new InventorySlot (Pickups.Slot.Primary, null));
		inventoryItems.Add (new InventorySlot (Pickups.Slot.Secondary, null));
		inventoryItems.Add (new InventorySlot (Pickups.Slot.Melee, null));
		inventoryItems.Add (new InventorySlot (Pickups.Slot.Grenade, null));
		inventoryItems.Add (new InventorySlot (Pickups.Slot.HealthItem, null));

		GetPrefs ();
	}

	void AddStartingItems () {
		if (startingItems != null) {
			if (startingItems.Length > 0) {
				for (int i = 0; i < startingItems.Length; i++) {
					GameObject item = Instantiate (startingItems [i].gameObject);
					AddItemToInventory (item);
				}
				EquipNextAvailable ();
			}
		}
	}

	public void AddItemToInventory(GameObject item) {
		// First, let's make sure it's an actual weapon
		Pickup pickup = item.GetComponent<Pickup>();
		if (pickup != null) {
			// Now, let's see if we can just put it in without swapping
			if (SlotIsFree (pickup.pickupSlot)) {
				// Slot is free, we can just put it in the inventory
				for (int i = 0; i < inventoryItems.Count; i++) {
					if (inventoryItems [i].slotType == pickup.pickupSlot) {
						if (inventoryItems [i].item == null) {
							AddToSlot (i, item);

							break;
						}
					}
				}
			} else {
				// We have to swap it with an item already there
				for (int i = 0; i < inventoryItems.Count; i++) {
					if (inventoryItems [i].slotType == pickup.pickupSlot) {
						EquipInventoryItem (inventoryItems [i].item);
						DropCurrentItem (true);

						AddToSlot (i, item);
					}
				}
			}
		}
		SetPrefs ();
		AfterInventorySlotChanged ();
	}

	// Get prefs
	void GetPrefs() {
		if(gameObject.name == "Player") {
			for(int i = 0; i < inventoryItems.Count; i++) {
				string pref = PlayerPrefs.GetString ("inventory_" + i);
				if (pref != "") {
					GameObject pickup = databasePickups.GetPickup (pref);
					if (pickup != null) {
						GameObject item = Instantiate (databasePickups.GetPickup (pref), weaponHolder.transform);

						item.name = pref;
						AddToSlot (i, item);
					} else {
						Debug.LogError ("Item " + pref + " does not exist in Database/Pickups");
					}
				}
			}

			// We'd rather have a primary weapon than a health pack in our hand when we spawn
			currentEquippedSlot = -1;
			EquipNextAvailable ();
		}
	}

	// Set prefs
	void SetPrefs() {
		if (gameObject.name == "Player") {
			for (int i = 0; i < inventoryItems.Count; i++) {
				GameObject item = inventoryItems [i].item;
				if (item != null) {
					PlayerPrefs.SetString ("inventory_" + i, item.name);
				} else {
					PlayerPrefs.SetString ("inventory_" + i, "");
				}

				PlayerPrefs.Save ();
			}
		}
	}

	// Used for when the player dies
	public GameObject PreservePistol() {
		if (inventoryItems [1].item != null) {
			inventoryItems [1].item.transform.SetParent (null);
			return inventoryItems [1].item;
		}
		return null;
	}

	// Call this to clear the inventory
	public void ClearInventory() {
		currentEquippedSlot = -1;
		for (int i = 0; i < inventoryItems.Count; i++) {
			inventoryItems [i].item = null;
		}
		Entity entity = GetComponent<Entity> ();
		entity.holding = null;
		for(int i = 0; i < weaponHolder.transform.childCount; i++) {
			Destroy (weaponHolder.transform.GetChild (0).gameObject);
		}
		SetPrefs ();
		AfterInventorySlotChanged ();
	}

	// Called before the inventory is slot is changed, before anything else happens
	void BeforeInventorySlotChanged() {
		Entity entity = GetComponent<Entity> ();

		// Prevent the item from being used, if it is
		if (entity.holding != null) {
			entity.holding.StopUse ();
		}

		// Stop any sounds that shouldn't be playing
		if (currentEquippedSlot != -1) {
			GameObject currentWeapon = weaponHolder.transform.Find (inventoryItems [currentEquippedSlot].item.name).gameObject;
			if (currentWeapon != null) {
				soundManager.ResetInstantiatedParent ("Gunshot", currentWeapon);
			}
		}
	}

	// Called after an inventory slot is changed
	void AfterInventorySlotChanged() {
		// change player anim
		if (animator != null && gameObject.name == "Player") {
			if (currentEquippedSlot != -1) {
				if (inventoryItems [currentEquippedSlot].item != null) {
					animator.Play ("player_wielding");
				} else {
					animator.Play ("player_not_wielding");
				}
			} else {
				animator.Play ("player_not_wielding");
			}
		}

		// Reset weapon holder position
		if (currentEquippedSlot != -1) {
			if (inventoryItems [currentEquippedSlot].slotType != Pickups.Slot.Primary && inventoryItems [currentEquippedSlot].slotType != Pickups.Slot.Secondary) {
				weaponHolder.transform.localEulerAngles = initialWeaponHolderRot;
			}
		}
	}

	void AddToSlot(int i, GameObject item) {
		// First, let's make sure the item is ready to go in the hand
		Destroy(item.GetComponent<Rigidbody>());
		item.GetComponent<BoxCollider> ().enabled = false;
		item.layer = 0;
		item.tag = "Untagged";

		// Then we can sort out the rest
		Pickup pickup = item.GetComponent<Pickup>();
		inventoryItems [i].item = item;
		item.transform.SetParent (weaponHolder.transform, false);
		item.transform.localPosition = Vector3.zero;
		item.transform.localEulerAngles = pickup.defaultRotation;
		currentEquippedSlot = i;

		// And finally we equip it
		EquipInventoryItem(item);

		AfterInventorySlotChanged ();
	}

	// Call this to drop the current item
	// forceDrop will drop regardless of if the item is secondary or not
	public void DropCurrentItem(bool forceDrop) {
		if (currentEquippedSlot != -1) {
			if (currentEquippedSlot != 1 || forceDrop) {
				BeforeInventorySlotChanged ();

				// First, let's drop the item
				GameObject item = inventoryItems [currentEquippedSlot].item;
				item.transform.parent = null;
				item.AddComponent<Rigidbody> ();
				item.GetComponent<BoxCollider> ().enabled = true;
				item.GetComponent<Rigidbody> ().mass = 0.15f;

				// Drop the item
				float dropForce = 30f;
				Pickups.DropDirection dropDir = item.GetComponent<Pickup> ().dropDirection;
				Vector3 dir = Vector3.zero;
				if (dropDir == Pickups.DropDirection.Forward) {
					dir = item.transform.forward;
				} else if (dropDir == Pickups.DropDirection.Left) {
					dir = -item.transform.right;
				} else if (dropDir == Pickups.DropDirection.Right) {
					dir = item.transform.right;
				}
				item.GetComponent<Rigidbody> ().AddForce (dir * dropForce);

				StartCoroutine (ResetTag (item));

				// Just to be sure, let's stop any reloading sounds too
				soundManager.StopInstantiatedSound ("Reload", item);

				// And finally free up the inventory slot
				GetComponent<Entity> ().holding = null;
				inventoryItems [currentEquippedSlot].item = null;
				currentEquippedSlot = -1;

				EquipNextAvailable ();
			} else {
				Debug.Log ("Cannot drop secondary weapon");
			}
			SetPrefs ();
			AfterInventorySlotChanged ();
		}
	}

	IEnumerator ResetTag(GameObject item) {
		item.tag = "DroppedInventoryItemNoPickup";
		yield return new WaitForSeconds(0.3f);
		item.layer = 12;
		item.tag = "DroppedInventoryItem";
	}

	// Destroys the item currently being held, such as if it was to break or be used
	public void RemoveCurrentHeldItem(bool equipNextAvailable) {
		// Is the entity holding something?
		if (currentEquippedSlot != -1) {
			RemoveItemAtIndex (currentEquippedSlot);
			if (equipNextAvailable) {
				EquipNextAvailable ();
			}

			AfterInventorySlotChanged ();
		}
	}

	// Equips the next available item, starting from primary and working down
	public void EquipNextAvailable() {
		for (int i = 0; i < inventoryItems.Count; i++) {
			if (inventoryItems [i].item != null) {
				EquipInventorySlot (i);
				AfterInventorySlotChanged ();
				break;
			}
		}
	}

	// Removes the item at specified array index
	void RemoveItemAtIndex(int index) {
		if (currentEquippedSlot != -1 && index < inventoryItems.Count) {
			Entity entity = GetComponent<Entity> ();

			// Physically delete the item; it's not needed now
			Destroy(inventoryItems [index].item);

			// May be unnecessary as we've deleted the reference, but it's good to be safe
			inventoryItems [index].item = null;
			entity.holding = null;
			currentEquippedSlot = -1;
		}
		SetPrefs ();
	}

	public void HideAllInventoryItems() {
		BeforeInventorySlotChanged ();

		for(int i = 0; i < inventoryItems.Count; i++) {
			if (inventoryItems [i].item != null) {
				inventoryItems [i].item.SetActive (false);
			}
		}
		GetComponent<Entity> ().holding = null;
		currentEquippedSlot = -1;

		AfterInventorySlotChanged ();
	}

	void EquipInventoryItem(GameObject item) {
		HideAllInventoryItems ();

		for(int i = 0; i < inventoryItems.Count; i++) {
			if (inventoryItems [i].item == item) {
				inventoryItems [i].item.SetActive (true);
				currentEquippedSlot = i;
				GetComponent<Entity> ().holding = inventoryItems [i].item.GetComponent<Pickup> ();

				// If a gun, set the entity of the gun to be this
				Gun gun = GetComponent<Entity> ().holding.GetComponent<Gun> ();
				if (gun != null) {
					gun.SetEntity (gameObject);
				}

				AfterInventorySlotChanged ();
				break;
			}
		}
	}

	public void EquipInventorySlot(int index) {
		if (index < inventoryItems.Count) {
			if (inventoryItems [index].item != null) {
				BeforeInventorySlotChanged ();

				HideAllInventoryItems ();
				inventoryItems [index].item.SetActive (true);
				currentEquippedSlot = index;
				GetComponent<Entity> ().holding = inventoryItems [index].item.GetComponent<Pickup> ();
			}
			AfterInventorySlotChanged ();
		}
	}

	public void EquipInventorySlot(Pickups.Slot slot) {
		// Check that the slot has an item
		if (SlotHasItem (slot)) {
			for (int i = 0; i < inventoryItems.Count; i++) {
				if (inventoryItems [i].slotType == slot) {
					// Call the main function, as this code would just make it harder to update if necessary
					EquipInventorySlot (i);
				}
			}
		}
	}

	// Scroll to the next inventory item available.  0 = up, 1 = down
	public void ScrollToNextItem(int dir) {
		// No point scrolling if we only have one weapon
		if (ActualInventoryCount() > 1) {
			int newSlot = currentEquippedSlot;
			int iterations = 0;

			if (dir == 0) {
				while (iterations < inventoryItems.Count) {
					if (newSlot + 1 > inventoryItems.Count - 1) {
						newSlot = 0;
					} else {
						newSlot++;
					}

					if (inventoryItems [newSlot].item != null) {
						EquipInventorySlot (newSlot);
						break;
					}

					iterations++;
				}
			} else {
				while (iterations < inventoryItems.Count) {
					if (newSlot - 1 < 0) {
						newSlot = inventoryItems.Count - 1;
					} else {
						newSlot--;
					}

					if (inventoryItems [newSlot].item != null) {
						EquipInventorySlot (newSlot);
						break;
					}

					iterations++;
				}
			}
		}
	}

	bool SlotIsFree(Pickups.Slot slot) {
		for (int i = 0; i < inventoryItems.Count; i++) {
			if (inventoryItems [i].slotType == slot) {
				if (inventoryItems [i].item == null) {
					return true;
				}
			}
		}
		return false;
	}

	public int ActualInventoryCount() {
		int count = 0;
		for(int i = 0; i < inventoryItems.Count; i++) {
			if (inventoryItems [i].item != null) {
				count++;
			}
		}
		return count;
	}

	public InventorySlot CurrentSlot() {
		if (currentEquippedSlot != -1) {
			return inventoryItems [currentEquippedSlot];
		}
		return null;
	}

	bool SlotHasItem(Pickups.Slot slot) {
		// Find the slot
		for (int i = 0; i < inventoryItems.Count; i++) {
			if (inventoryItems [i].slotType == slot) {
				if (inventoryItems [i].item != null) {
					return true;
				}
			}
		}
		return false;
	}
}