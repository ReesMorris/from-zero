using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBindManager : MonoBehaviour {

	public TMP_Text mainMenuLabel;

	private InputManager inputManager;
	private bool isBinding;
	public bool IsBinding {
		get {
			return isBinding;
		}
	}

	private TMP_Text currentText;
	private LanguageManager languageManager;
	private string prevText;
	private string key;

	void Start() {
		inputManager = GetComponent<InputManager> ();
		languageManager = GetComponent<LanguageManager> ();
		isBinding = false;
	}

	void OnGUI() {
		Event e = Event.current;
		if (isBinding) {
			if (e.isKey) {
				// keyboard keys
				if (e.keyCode == KeyCode.Escape) {
					StopBinding ();
				} else {
					if (e.keyCode != KeyCode.None) {
						FinishBind (e.keyCode);
					}
				}
			} else if (e.isMouse) {
				// mouse keys
				if (e.button == 0) {
					FinishBind (KeyCode.Mouse0);
				}
				if (e.button == 1) {
					FinishBind (KeyCode.Mouse1);
				}
				if (e.button == 2) {
					FinishBind (KeyCode.Mouse2);
				}
				if (e.button == 3) {
					FinishBind (KeyCode.Mouse3);
				}
				if (e.button == 4) {
					FinishBind (KeyCode.Mouse4);
				}
				if (e.button == 5) {
					FinishBind (KeyCode.Mouse5);
				}
				if (e.button == 6) {
					FinishBind (KeyCode.Mouse6);
				}
			} else {
				// command keys
				if (Input.GetKey (KeyCode.LeftShift)) {
					FinishBind (KeyCode.LeftShift);
				}
				if (Input.GetKey (KeyCode.RightShift)) {
					FinishBind (KeyCode.RightShift);
				}
				if (Input.GetKey (KeyCode.LeftControl)) {
					FinishBind (KeyCode.LeftControl);
				}
				if (Input.GetKey (KeyCode.RightControl)) {
					FinishBind (KeyCode.RightControl);
				}
				if (Input.GetKey (KeyCode.LeftCommand)) {
					FinishBind (KeyCode.LeftCommand);
				}
				if (Input.GetKey (KeyCode.RightCommand)) {
					FinishBind (KeyCode.RightCommand);
				}
				if (Input.GetKey (KeyCode.LeftAlt)) {
					FinishBind (KeyCode.LeftAlt);
				}
				if (Input.GetKey (KeyCode.RightAlt)) {
					FinishBind (KeyCode.RightAlt);
				}
				if (Input.GetKey (KeyCode.CapsLock)) {
					FinishBind (KeyCode.CapsLock);
				}
			}
		}
	}

	// Begin a keybind, preventing other keys from being bound and stopping any current bindings
	public void StartBind(KeyBindButton button) {
		if (!isBinding) {
			isBinding = true;
			UpdateButtonText ();
			currentText = button.displayText;
			if (currentText != null) {
				key = button.inputKey;
				prevText = currentText.text;
				currentText.text = "";
			} else {
				Debug.LogError ("Missing text component of this button");
			}
		}
	}

	void FinishBind(KeyCode newKey) {
		inputManager.UpdateInputKey (key, newKey);
		prevText = newKey.ToString();
		StopBinding ();
	}

	public void StopBinding() {
		if (isBinding) {
			StartCoroutine ("StopBindingDelay");
		}
	}

	void UpdateButtonText() {
		if (isBinding) {
			mainMenuLabel.text = languageManager.GetString ("ui_button_cancel_keybind");
		} else {
			mainMenuLabel.text = languageManager.GetString ("ui_button_menu");
		}
	}

	public string GetDefaultKey(string inputKey) {
		return inputManager.GetKeyValue (inputKey);
	}

	IEnumerator StopBindingDelay() {
		currentText.text = prevText;
		yield return new WaitForSeconds(0.1f); // prevents going to the main menu when pressing ESC
		isBinding = false;
		UpdateButtonText ();
	}
}
