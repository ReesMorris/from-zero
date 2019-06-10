using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Story_FireIntensity : MonoBehaviour {

	public float baseIntensity;
	public float maxIntensity;
	public float flickerRate;
	public float flickerSpeed;

	private Light flickerLight;

	void Start () {
		flickerLight = GetComponent<Light> ();

		StartCoroutine ("Flicker");
	}
	
	IEnumerator Flicker() {
		while (true) {
			flickerLight.intensity = baseIntensity + Mathf.Sin (Time.time * flickerRate);
			if (flickerLight.intensity > maxIntensity) {
				flickerLight.intensity = maxIntensity;
			}

			yield return new WaitForSeconds (flickerSpeed);
		}
	}
}
