using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawner : MonoBehaviour {

	[Header("GameObjects")]
	public GameObject enemyPrefab; // prefab of the enemy to spawn
	public Transform spawner; // position where to spawn the  enemy

	[Header("Spawner Config")]
	public bool spawnOnStart; // if true, triggers the spawn function as soon as the game starts (unless delay)
	public float spawnDelay; // One enemy every "spawnDelay" seconds
	public float maxEnemies; // Maximum number of enemies

	[Header("Entity Config")]
	public bool customConfig = false; // incase we have spawners that don't need to modify this
	public int startingHealth;
	public int healthRandomOffset; // a random number to make health slightly different per enemy
	public int searchRangeEmpty = -1;
	public int searchRangeFound = -1;
	public float speed = -1;

	void Start() {
		if (spawnOnStart) {
			Spawn ();
		}
	}

	// start the coroutine for the spawning
	// public function that can be called from other scripts
	public void Spawn() {
		StartCoroutine ("SpawnEnemy");
	}

	// Function that generates one enemy every spawnDelay seconds, until the number of
	// enemy spawned reaches maxEnemies
	IEnumerator SpawnEnemy() {
		for (int i = 0; i < maxEnemies; i++) {
			GameObject enemy = Instantiate (enemyPrefab, spawner.position, spawner.rotation);
			if (customConfig) {
				Entity entity = enemy.GetComponent<Entity> ();
				Pathfinding pathfinding = enemy.GetComponent<Pathfinding> ();
				NavMeshAgent navMeshAgent = enemy.GetComponent<NavMeshAgent> ();

				entity.name = enemyPrefab.name;

				// Set the new health value
				float newHealth = startingHealth + (Random.Range (-healthRandomOffset, healthRandomOffset));
				if (newHealth < 1) {
					// Don't want the entity to die straight away, so let's reset that..
					newHealth = startingHealth;
				}
				entity.SetMaxHealth (newHealth);

				if(searchRangeEmpty != -1) {
					pathfinding.searchRangeEmpty = searchRangeEmpty;
				}
				if(searchRangeFound != -1) {
					pathfinding.searchRangeFound = searchRangeFound;
				}
				if (speed != -1) {
					navMeshAgent.speed = speed;
				}
			}
			yield return new WaitForSeconds (spawnDelay);
		}
	}
		
}

	
