using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseManager : MonoBehaviour {

	[Header("Modal")]
	public GameObject modal;
	public LocalisedText modalTitle;
	public LocalisedText modalBody;
	public LocalisedText modalAccept;
	public LocalisedText modalReject;
	private string modalIdentifier;

	private InputManager inputManager;
	private UIManager uiManager;
	private bool isPaused;
	public bool IsPaused {
		get {
			return isPaused;
		}
	}
	private GameObject currentWarning;
	private ConsoleManager consoleManager;

	void Start() {
		inputManager = GetComponent<InputManager> ();
		uiManager = GetComponent<UIManager> ();
		consoleManager = GetComponent<ConsoleManager> ();
	}

	void Update () {
		if (inputManager.InputMatchesKeyup ("UI_PAUSE_GAME")) {
			// only toggle pause if no UI menus are open
			if (!consoleManager.ConsoleOpen) {
				if (!modal.activeSelf) {
					TogglePause ();
				} else {
					modal.SetActive (false);
				}
			}
		}
	}

	// Toggle the game between paused and unpaused
	public void TogglePause() {
		isPaused = !isPaused;
		uiManager.pauseMenu.SetActive (!uiManager.pauseMenu.activeSelf);

		Time.timeScale = 1f;
		if (isPaused) {
			Time.timeScale = 0f;
		}
	}

	// Called when an item in the pause menu list is clicked
	public void OnMenuItemClick(PauseMenuChoice choice) {
		if (choice.identifier == PauseMenuChoice.Identifiers.resume) {
			modal.SetActive (false);
			TogglePause ();
		} else {
			ShowModal (choice);
		}
	}

	// Called when the confirm button is pressed on the modal
	public void OnModalAcceptClick() {
		switch (modalIdentifier) {
		case "settings":
			Settings ();
			break;
		case "mainMenu":
			MainMenu ();
			break;
		case "quit":
			Quit ();
			break;
		}
	}

	// Called when the reject button is pressed on the modal
	public void OnModalRejectClick() {
		modal.SetActive (false);
	}

	// Displays the modal window with the texts for the corresponding choice
	void ShowModal(PauseMenuChoice choice) {
		modal.SetActive (true);
		SetPositions ();
		modalTitle.UpdateText (choice.titleKey);
		modalBody.UpdateText (choice.dialogueKey);
		modalAccept.UpdateText (choice.acceptButtonKey);
		modalReject.UpdateText (choice.rejectButtonKey);
		modalIdentifier = choice.identifier.ToString ();
	}

	// Loads the main menu scene
	void MainMenu() {
		SceneManager.LoadScene (0); // main menu must be scene 0, no?
	}

	// Loads the settings scene (temporarily; we'll be fixing this soon)
	void Settings() {
		PlayerPrefs.SetInt ("showSettings", 1);
		SceneManager.LoadScene (0);
	}

	// Unity function; called when the game is quit
	void OnApplicationQuit() {
		PlayerPrefs.SetInt ("showSettings", 0); // so we don't abritrarily open the settings menu if we quit
	}

	// Function to quit the game
	void Quit() {
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}

	// Updates the offset positions for text in the modal; it breaks from time to time
	void SetPositions() {
		RectTransform rt1 = modalTitle.GetComponent<RectTransform> ();
		RectTransform rt2 = modalBody.GetComponent<RectTransform> ();
		rt1.offsetMax = new Vector2(rt1.offsetMax.x, 0);
		rt2.offsetMax = new Vector2(rt2.offsetMax.x, 0);
	}
}
