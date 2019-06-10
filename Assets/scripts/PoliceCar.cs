using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceCar : MonoBehaviour {

	[Header("Lighting")]
	public GameObject[] lights;
	public float duration;

	void Start () {
		InitLights ();
		StartCoroutine (PoliceLights ());
	}

	// Set some lights on, some lights off
	void InitLights() {
		for (int i = 0; i < lights.Length; i++) {
			lights [i].SetActive (true);
			if (i % 2 == 0) {
				lights [i].SetActive (false);
			}
		}
	}

	// Cycle beween the police lights
	IEnumerator PoliceLights() {
		while (true) {
			for (int i = 0; i < lights.Length; i++) {
				lights [i].SetActive (!lights [i].activeSelf);
			}
			yield return new WaitForSeconds (duration);
		}
	}
}
