using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {

	public bool tutorialEnabled = true;
	public GameObject tutorialUI;
	public GameObject tutorialText;

	private TextMeshProUGUI tmp;
	private LanguageManager lm;
	private List<Tutorial> tutorialQueue;
	private bool tutorialShowing = false;

	void Start() {
		Init ();
		tmp = tutorialText.GetComponent<TextMeshProUGUI> ();

		tutorialUI.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (20f, -20f);
		HideTutorialUI ();
	}

	void Update() {
		if (tutorialQueue.Count > 0 && !tutorialShowing) {
			StartCoroutine ("DisplayTutorial");
		}
	}

	public void QueueTutorial(string key, float duration) {
		Init ();

		string tutorialText = lm.GetString (key);
		if (tutorialText != "") {
			tutorialQueue.Add (new Tutorial (tutorialText, duration));
		}
	}

	public void ShowTutorialUI() {
		tutorialUI.SetActive (true);
	}

	public void HideTutorialUI() {
		tutorialUI.SetActive (false);
	}

	// Reset which tutorials have been shown and which have not
	public void ResetTutorialProgress() {
		// not sure how to do this yet..
	}

	void Init() {
		if (lm == null) {
			lm = GameObject.Find ("GameManager").GetComponent<LanguageManager> ();
		}
		if (tutorialQueue == null) {
			tutorialQueue = new List<Tutorial> ();
		}
	}

	IEnumerator DisplayTutorial() {
		tutorialShowing = true;

		// Set the text and show the UI
		tmp.SetText (tutorialQueue[0].text);
		ShowTutorialUI ();

		// Wait for tutorial length
		yield return new WaitForSeconds (tutorialQueue[0].duration);
		HideTutorialUI ();

		// Remove this tutorial from the queue
		tutorialQueue.RemoveAt (0);
		tutorialShowing = false;
	}
}
