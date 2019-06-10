using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthItem : MonoBehaviour {

	public float healAmount;
	public float timeToHeal;
	private bool healing = false;
	private float timePassed = 0f;

	public void StartHealing() {
		StartCoroutine ("Heal");
	}

	public void StopHealing() {
		if (healing) {
			Transform parent = transform.parent.parent;
			Entity entity = parent.GetComponent<Entity> ();

			entity.IsHealing = false;
			healing = false;

			StopCoroutine ("Heal");
			timePassed = 0f;
		}
	}

	IEnumerator Heal() {
		Transform parent = transform.parent.parent;
		Entity entity = parent.GetComponent<Entity> ();
		Inventory inventory = parent.GetComponent<Inventory> ();

		if (!healing) {
			if (entity.CanHeal()) {
				healing = true;
				entity.IsHealing = true;

				while (timePassed < timeToHeal) {
					yield return new WaitForSeconds (0.1f);
					timePassed += 0.1f;
					print("Healing.. " + (timeToHeal - timePassed) + "s left");
				}

				// Make sure that the entity is real
				if (entity != null) {
					// Tell the entity that they're no longer healing
					entity.IsHealing = false;

					// First, let's increase the health
					entity.IncreaseHealth (healAmount);

					// Next, let's get rid of this item
					inventory.RemoveCurrentHeldItem (true);
				} else {
					Debug.LogError ("Can't find person holding this MedKit. Are they an entity?");
				}
			} else {
				print ("I can't heal right now.");
			}
		}
	}
}
