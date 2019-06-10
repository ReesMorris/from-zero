using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainModules : MonoBehaviour {

	public GameObject moduleContainer;
	[Range(0f, 0.5f)]
	public float moveSpeed;
	public float moduleDistance;
	public TrainModule[] modules;

	private List<GameObject> spawned;
	private PauseManager pauseManager;
	private float nextOffset;

	void Start () {
		pauseManager = GameObject.Find ("GameManager").GetComponent<PauseManager> ();
		spawned = new List<GameObject> ();
		PreSpawn ();
		StartCoroutine ("UpdateModules");
	}

	void Update() {
		float newSpeed = moveSpeed;
		if (pauseManager.IsPaused)
			newSpeed = 0;
		moduleContainer.transform.localPosition += new Vector3 (newSpeed * 2.75f, 0, -newSpeed);
	}

	void PreSpawn() {
		for (int i = 0; i < 30; i++) {
			SpawnOne ();
		}
	}

	void SpawnOne() {
		GameObject newModule = Instantiate (ChooseModule ());
		newModule.transform.parent = moduleContainer.transform;
		newModule.transform.localEulerAngles = new Vector3 (0, 0, 0);
		newModule.transform.localPosition = new Vector3 (0, 0, nextOffset);
		nextOffset -= moduleDistance;
		spawned.Add (newModule);
	}

	GameObject ChooseModule() {
		int totalChance = 0;
		for (int i = 0; i < modules.Length; i++) {
			totalChance += modules [i].chanceOfAppear;
		}
		int random = Random.Range (0, totalChance);
		for (int i = 0; i < modules.Length; i++) {
			random -= modules [i].chanceOfAppear;
			if (random <= 0) {
				return modules [i].module;
			}
		}
		return modules [0].module;
	}

	IEnumerator UpdateModules() {
		float freq = 2.4f - moveSpeed;
		while (true) {
			if (moveSpeed > 0) {
				yield return new WaitForSeconds (freq);
				GameObject remove = spawned [0];
				spawned.RemoveAt (0);
				Destroy (remove);
				SpawnOne ();
			} else {
				yield return new WaitForSeconds (1f);
			}
		}
	}

	public void SlowDown() {
		StartCoroutine ("Slow");
	}

	public void SpeedUp() {
		StartCoroutine ("Speed");
	}

	IEnumerator Slow() {
		while(moveSpeed > 0.02f) {
			moveSpeed -= 0.005f;
			yield return new WaitForSeconds (0.1f);
		}
	}

	IEnumerator Speed() {
		while(moveSpeed < 0.067f) {
			moveSpeed += 0.005f;
			yield return new WaitForSeconds (0.1f);
		}
	}
}
