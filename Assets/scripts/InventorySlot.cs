using UnityEngine;

public class InventorySlot {

	public Pickups.Slot slotType;
	public GameObject item;

	public InventorySlot(Pickups.Slot type, GameObject go) {
		slotType = type;
		item = go;
	}
}
