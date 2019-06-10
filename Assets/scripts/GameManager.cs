using UnityEngine;

public class GameManager : MonoBehaviour {

	public bool canSkipCutscenes = true;

	void Start() {
		if (GetComponent<CutsceneManager> () == null) {
			gameObject.AddComponent<CutsceneManager> ();
			LogMessage ("CutsceneManager");
		}
		if (GetComponent<LanguageManager> () == null) {
			gameObject.AddComponent<LanguageManager> ();
			LogMessage ("LanguageManager");
		}
		if (GetComponent<SoundManager> () == null) {
			gameObject.AddComponent<SoundManager> ();
			LogMessage ("SoundManager");
		}
		if (GetComponent<CameraController> () == null) {
			gameObject.AddComponent<CameraController> ();
			LogMessage ("CameraController");
		}
		if (GetComponent<TutorialManager> () == null) {
			gameObject.AddComponent<TutorialManager> ();
			LogMessage ("TutorialManager");
		}
		if (GetComponent<SubtitleManager> () == null) {
			gameObject.AddComponent<SubtitleManager> ();
			LogMessage ("SubtitleManager");
		}
	}

	void LogMessage(string scriptName) {
		Debug.Log ("GameManager: Added {"+scriptName+".cs} script");
	}
}
