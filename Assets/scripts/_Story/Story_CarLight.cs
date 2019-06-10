using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story_CarLight : MonoBehaviour {

	public float delay;
	public GameObject[] lights;

	public void StartCycle() {
		StartCoroutine ("Cycle");
	}

	public void StopCycle() {
		StopCoroutine ("Cycle");
		foreach (GameObject light in lights) {
			light.SetActive (false);
		}
	}

	IEnumerator Cycle() {
		while (true) {
			foreach (GameObject light in lights) {
				light.SetActive (!light.activeSelf);
			}
			yield return new WaitForSeconds (delay);
		}
	}
}
