using System.Collections;
using UnityEngine;

public class AudioObject : MonoBehaviour {

	public bool bulletCanBreak;

	[Header("On Bullet Collide")]
	public AudioSource audioSource;
	public AudioClip brokenNoise;

	private GameObject tempStatic;

	// Called when a collision takes place
	void OnCollisionEnter(Collision collision) {
		if(bulletCanBreak) {
			if (collision.gameObject.tag == "Bullet") {
				// Play static
				if (tempStatic == null) {
					tempStatic = new GameObject ();
					tempStatic.name = "Static SFX";
					tempStatic.AddComponent<Sound> ();

					Sound tempSound = tempStatic.GetComponent<Sound> ();
					tempSound.type = 2;
					tempSound.areaVolume = audioSource.GetComponent<Sound> ().areaVolume;

					tempStatic.AddComponent<AudioSource> ();
					tempStatic.transform.parent = gameObject.transform;
					tempStatic.transform.localPosition = Vector3.zero;

					AudioSource tempSource = tempStatic.GetComponent<AudioSource> ();
					tempSource.clip = brokenNoise;
					tempSource.loop = true;
					tempSource.Play ();

					StartCoroutine ("Destroy");
				}
			}
		}
	}

	IEnumerator Destroy() {
		yield return new WaitForSeconds (0.3f);
		float volumeDecrease = 0.04f;

		while (tempStatic.GetComponent<AudioSource> ().volume > 0) {
			audioSource.pitch = Random.Range(0.25f, 1.75f);
			audioSource.volume -= volumeDecrease;
			tempStatic.GetComponent<AudioSource> ().volume -= (volumeDecrease / 1.9f);
			yield return new WaitForSeconds (0.1f);
		}

		Destroy (audioSource.transform.gameObject);
		Destroy (tempStatic);
		Destroy (this);
	}
}
