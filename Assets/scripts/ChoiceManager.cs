using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ChoiceManager : MonoBehaviour {

	public GameObject choiceContainer;
	public GameObject timerContainer;
	public GameObject choiceButtonPrefab;
	public Image timer;

	private LanguageManager lm;
	private float initialTime;
	private float timeLeft;
	private int defaultChoice;
	private bool choicePending = false;
	public bool ChoicePending {
		get {
			return choicePending;
		}
	}

	void Start() {
		lm = GetComponent<LanguageManager> ();
		Reset ();
	}

	// Add the choice button and set its value. More efficient as you can now set up an
	// entire choice sequence by calling one function, rather than many
	// Set decisionTime to -1 to prevent the timer from showing / counting down
	public void SetupChoiceSequence(float decisionTime, int _defaultChoice, params string[] choiceKeys) {
		Reset ();

		// This prevents the default choice from being -1, which means no choice is made so it will get stuck otherwise
		if (_defaultChoice == -1 && decisionTime != -1) {
			_defaultChoice = int.MaxValue; // probably won't have this many choices
		}

		choicePending = true;
		System.Array.Reverse(choiceKeys); // put the choices in the order they are input
		for (int i = 0; i < choiceKeys.Length; i++) {
			// Make the new button
			GameObject newChoice = Instantiate (choiceButtonPrefab);
			newChoice.transform.SetParent (choiceContainer.transform, false);
			newChoice.GetComponent<TMP_Text> ().text = lm.GetString(choiceKeys[i]);
			newChoice.name = "Choice";
			newChoice.transform.SetAsFirstSibling ();

			// Set its value to the parameter passed
			newChoice.GetComponent<ChoiceButton> ().SetValue (choiceKeys.Length - 1 - i); // (as the array is backwards, the option indexes will also be)
		}

		// Show the choice container
		choiceContainer.SetActive(true);

		// Set the values for the IEnumerator to run, and set it off
		initialTime = decisionTime;
		timeLeft = decisionTime;
		defaultChoice = _defaultChoice;
		timerContainer.SetActive (false);
		if (decisionTime != -1) {
			timerContainer.SetActive (true);
			StartCoroutine ("ReduceTimer");
		}
	}

	// Returns true if the player has made a choice
	public bool ChoiceIsMade() {
		// Stops a black screen if the player tries to skip a cutscene
		//storyManager.StopCutsceneSkip();

		if(PlayerPrefs.GetInt ("playerChoice") != -1) {
			return true;
		}
		return false;
	}

	// Call this after a choice has been made, so another can be made later
	public void Reset() {
		Time.timeScale = 1f;
		choicePending = false;
		StopCoroutine ("ReduceTimer");
		PlayerPrefs.SetInt ("playerChoice", -1);

		// Enable this so we can see the children
		choiceContainer.SetActive(true);

		// Destroy all child buttons
		foreach (Transform child in choiceContainer.transform) {
			if (child.name == "Choice") {
				Destroy(child.gameObject);
			}
		}

		// Hide the choice container
		choiceContainer.SetActive(false);
	}

	// Returns the current choice, and then resets the choices
	public int GetChoice() {
		int choice = PlayerPrefs.GetInt ("playerChoice");
		Reset ();
		return choice;
	}

	IEnumerator ReduceTimer() {
		while (timeLeft > 0f) {
			yield return new WaitForSeconds (0.01f);
			timeLeft -= 0.01f;
			timer.fillAmount = timeLeft / initialTime;
		}
		PlayerPrefs.SetInt ("playerChoice", defaultChoice);
	}
}