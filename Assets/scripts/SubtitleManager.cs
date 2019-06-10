using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SubtitleManager : MonoBehaviour {

	public bool subtitlesEnabled = true;
	public TextMeshProUGUI subtitleText;

	private LanguageManager lm;
	private IEnumerator currentSubs;
	private bool subtitlesRunning;
	private AudioSource currentAudioSource;

	void Start () {
		lm = GameObject.Find ("GameManager").GetComponent<LanguageManager> ();
		subtitlesRunning = false;

		ClearSubtitles ();
	}

	public void StartSubtitles(AudioSource audio) {
		subtitlesRunning = true;
		currentSubs = Subtitles (audio);
		StartCoroutine (currentSubs);
	}

	public void StopSubtitles(AudioSource audio) {
		if (subtitlesRunning && audio == currentAudioSource) {
			StopCoroutine (currentSubs);
			ClearSubtitles ();
			subtitlesRunning = false;
		}
	}

	public void SetSubtitles(string key) {
		if (subtitlesEnabled) {
			string text = lm.GetString (key);
			subtitleText.text = text;
		}
	}

	public void ClearSubtitles() {
		subtitleText.text = "";
	}

	IEnumerator Subtitles(AudioSource audio) {
		Sound sound = audio.GetComponent<Sound> ();
		currentAudioSource = audio;
		int index = 0;

		SetTimings (sound);

		while (audio.isPlaying && index < sound.subtitles.Length) {
			// Below is the new subtitling system. It is more efficient as it doesn't rely on any previous
			// subtitles to have finished before showing the next one, so it should be more reliable.

			// Subtitle has finished
			if (currentAudioSource.time > sound.subtitles [index].EndTime) {
				ClearSubtitles ();
				index++;
			}

			// Subtitle is going
			if (currentAudioSource.time >= sound.subtitles [index].StartTime) {
				SetSubtitles (sound.subtitles [index].key);
			}

			yield return new WaitForSeconds (0.1f);


			/*
			// Below is the old subtitling system, only use if a major issue is discovered with the above.
			// The below system is inefficient as it can cause delayed subtitles if the user's computer is slow
			// or if a clock error misses a time. It will mean all subtitles display for time they should, but
			// it means that one subtitle going out of sync will result in all future ones going out of sync as well.

			// Delay before subtitles show
			if (index == 0) {
				// Wait for the start of the first subtitle
				yield return new WaitForSeconds (sound.subtitles [index].StartTime);
			} else {
				// The delay of blank space is equal to the start time of this subtitle, subtract the end time of the last subtitle
				yield return new WaitForSeconds (sound.subtitles [index].StartTime - sound.subtitles [index-1].EndTime);
			}

			// Show the subtitle
			SetSubtitles (sound.subtitles [index].key);

			// Wait for the duration of the subtitle
			yield return new WaitForSeconds (sound.subtitles [index].EndTime - sound.subtitles [index].StartTime);

			// Hide the subtitle and increase the index
			ClearSubtitles ();
			index++;
			*/
		}
		ClearSubtitles ();
	}

	public void SetTimings(Sound sound) {
		for (int i = 0; i < sound.subtitles.Length; i++) {
			Subtitle subtitle = sound.subtitles [i];
			if (i == 0) {
				subtitle.StartTime = subtitle.startDelay;
				subtitle.EndTime = subtitle.duration + subtitle.startDelay;
			} else {
				Subtitle previous = sound.subtitles [i - 1];
				subtitle.StartTime = previous.EndTime + subtitle.startDelay;
				subtitle.EndTime = subtitle.StartTime + subtitle.duration;
			}
		}
	}
}
