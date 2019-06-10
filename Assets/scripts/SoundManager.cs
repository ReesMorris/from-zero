using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
	
	public SoundTypes[] soundTypes;
	private SubtitleManager subtitleManager;

	public delegate void OnVolumeChanged(); // let other scripts know that our volume has been changed
	public OnVolumeChanged volumeChanged;

	void Start() {
		if (GameObject.Find ("GameManager") != null) {
			subtitleManager = GameObject.Find ("GameManager").GetComponent<SubtitleManager> ();
		}
	}

	public void PlaySound(AudioSource audio, float delay) {
		StartCoroutine (PlaySoundDelay (audio, delay));
	}

	public void StopSound(AudioSource audio) {
		subtitleManager.StopSubtitles (audio);
		audio.Stop ();
	}

	public void ToggleSound(AudioSource audio) {
		if (audio.isPlaying)
			audio.Pause ();
		else
			audio.Play ();
	}

	public void FadeSound(AudioSource audio, float time) {
		StartCoroutine(FadeOutSound(audio, time));
	}

	public void StopSoundsInDirectory(GameObject directory) {
		for (int i = 0; i < directory.transform.childCount; i++) {
			GameObject child = directory.transform.GetChild (i).gameObject;
			AudioSource audio = child.GetComponent<AudioSource> ();

			if (audio.isPlaying) {
				StopSound (audio);
			}
		}
	}

	IEnumerator PlaySoundDelay(AudioSource audio, float delay) {
		Sound sound = audio.GetComponent<Sound> ();

		// Check that the sound component exists
		if (sound == null) {
			Debug.LogError (audio.name + ": Sound script missing!");
		}

		// Wait for the delay
		yield return new WaitForSeconds (delay);

		// Stop other sounds from playing, if necessary
		if (soundTypes[sound.type].stopOthers) {
			StopSoundsInDirectory (soundTypes [sound.type].directory);
		}

		// Play the audio file
		audio.Play ();

		// Show subtitles
		if (sound.showSubtitles && sound.subtitles.Length > 0) {
			subtitleManager.StartSubtitles (audio);
		}
	}

	// Call this to instantiate an audio clip at a specific world point, and destroy/loop it after it's finished
	public void InstantiateSound(string name, GameObject parent, AudioClip audioClip, int areaVolume, bool looping, bool destroyOthers, bool is3D) {
		if (destroyOthers) {
			StopInstantiatedSound (name, parent);
		}
		GameObject audio = new GameObject(name);
		audio.transform.SetParent (parent.transform);
		audio.transform.localPosition = Vector3.zero;

		audio.AddComponent<AudioSource> ();
		audio.AddComponent<Sound> ();

		Sound sound = audio.GetComponent<Sound> ();
		sound.type = 1;
		sound.use3dSound = is3D;
		sound.areaVolume = areaVolume;
		if (!looping) {
			sound.destroyOnComplete = true;
		}

		AudioSource audioSource = audio.GetComponent<AudioSource> ();
		audioSource.clip = audioClip;
		if (looping) {
			audioSource.loop = true;
		}
		audioSource.Play ();
	}

	// Destroys a sound that has been created, for whatever reason
	public void StopInstantiatedSound(string name, GameObject parent) {
		foreach(Transform child in parent.transform) {
			if (child.name == name) {
				// This checks to make sure that we are infact deleting a sound object,
				// in case there is a normal GameObject which (somehow) shares the same name
				if (child.GetComponent<AudioSource> () != null) {
					Destroy (child.gameObject);
				}
			}
		}
	}

	// Sets the parent of an instantiated gameobject to be null
	public void ResetInstantiatedParent(string name, GameObject parent) {
		foreach(Transform child in parent.transform) {
			if (child.name == name) {
				// This checks to make sure that we are infact deleting a sound object,
				// in case there is a normal GameObject which (somehow) shares the same name
				if (child.GetComponent<AudioSource> () != null) {
					child.transform.SetParent (null);
				}
			}
		}
	}

	// Fades out a sound
	IEnumerator FadeOutSound(AudioSource audio, float time) {
		while (audio.volume > 0f) {
			audio.volume -= 0.01f;
			yield return new WaitForSeconds (time / (audio.volume * 100)); // number of iterations it will take this sound to 0
		}

		// Reset the sound for later use, just in case
		audio.Stop();
		audio.volume = 1;
	}
}
