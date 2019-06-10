using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {

	public int index;
	public AudioSource[] audioSource;

	private bool interacting;
	private LocalisedName localisedName;
	private string originalName;
	private SoundManager soundManager;
	private StoryManager storyManager;

	void Start() {
		localisedName = GetComponent<LocalisedName> ();
		originalName = localisedName.key;
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
		storyManager = GameObject.Find ("GameManager").GetComponent<StoryManager> ();
	}

	public void OnInteraction() {
		if (!interacting) {
			StartCoroutine ("Interact");
		}
	}

	IEnumerator Interact() {
		interacting = true;

		yield return new WaitForSeconds (0f);

		// Open the door in the bar
		if (index == 0) {
			int state = PlayerPrefs.GetInt ("key_state");
			if (state == 0) {
				localisedName.UpdateText ("interactable_door_locked");
				yield return new WaitForSeconds (2f);
				localisedName.UpdateText (originalName);
			} else if (state == 1) {
				soundManager.PlaySound (audioSource [0], 0f);
				localisedName.UpdateText ("interactable_door_locked");
				yield return new WaitForSeconds (2f);
				localisedName.UpdateText (originalName);
			} else {
				soundManager.PlaySound (audioSource [1], 0f);
				storyManager.StartStory ("Story_Bar_2");
			}
		}

		// Pick up the key
		if (index == 1) {
			PlayerPrefs.SetInt ("key_state", 2);
			Destroy (gameObject);
		}

		// Payphone
		if (index == 2) {
			storyManager.StartStory ("Story_Fuel_Station_3");
		}

		interacting = false;
	}
}
