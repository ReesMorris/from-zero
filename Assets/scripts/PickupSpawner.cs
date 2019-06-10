using UnityEngine;

public class PickupSpawner : MonoBehaviour {

	public bool spawnOnStart;
	public GameObject item;

	void Start () {
		// Should this spawner spawn when the scene loads?
		if (spawnOnStart) {
			Spawn ();
		}
	}

	// Call this function to trigger the spawner to spawn
	public void Spawn(){
		if (item != null) {
			// Instantiate at current position
			GameObject newItem = Instantiate (item, transform.position, Quaternion.identity);
			newItem.name = item.name;
		} else {
			Debug.LogError ("Cannot spawn from pickup spawner; it has no item.");
		}
	}

}
