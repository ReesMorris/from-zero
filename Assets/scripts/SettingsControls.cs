using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class SettingsControls : MonoBehaviour {

	public GameObject titleTextPrefab;
	public GameObject bindingPanel;
	public GameObject keyBindPrefab;
	private LanguageManager languageManager;
	private InputManager inputManager;
	private string lastAddedPrefix;
	private float lastSetupTime;

	void Start() {
		GameObject scene = GameObject.Find("Main Menu");
		if (scene != null) {
			// Main menu
			inputManager = GameObject.Find("Main Menu").GetComponent<InputManager>();
			languageManager = GameObject.Find("Main Menu").GetComponent<LanguageManager>();
		} else {
			// In game
			inputManager = GameObject.Find("GameManager").GetComponent<InputManager>();
			languageManager = GameObject.Find("GameManager").GetComponent<LanguageManager>();
		}

		SetupPanel ();
	}

	// incase language is changed
	public void SetupPanel() {
		if (Time.time > lastSetupTime + 0.2f) { // without this, it will be instantiated twice when first opened (no idea why!)
			lastSetupTime = Time.time;
			foreach (Transform child in bindingPanel.transform) {
				GameObject.Destroy (child.gameObject);
			}

			StartCoroutine ("AddInputs");
		}
	}

	IEnumerator AddInputs() {
		yield return new WaitForSeconds (0.1f);

		// Inputs are here, let's add them!
		for (int i = 0; i < inputManager.Inputs.Count; i++) {
			CustomInput input = inputManager.Inputs [i];

			// If we don't want to show this control (for whatever reason)
			if (!input.displayInControls) {
				continue;
			}

			// Does it need a title?
			string[] prefix = input.name.Split('_');
			if (lastAddedPrefix != prefix[0]) {
				lastAddedPrefix = prefix [0];
				GameObject title = Instantiate (titleTextPrefab, bindingPanel.transform);
				title.GetComponent<TMP_Text> ().text = languageManager.GetString("control_" + prefix [0].ToLower());
			}

			// Panel itself
			GameObject newPanel = Instantiate (keyBindPrefab, bindingPanel.transform);
			newPanel.name = input.name;

			// Disable the button
			if (!input.allowRebinding) {
				newPanel.GetComponent<Button> ().interactable = false;
				newPanel.transform.Find ("Button").GetComponent<Image> ().color = new Color (0.8f, 0.8f, 0.8f);
			}

			KeyBindButton kbb = newPanel.GetComponent<KeyBindButton> ();
			kbb.inputKey = input.name;
			newPanel.transform.Find ("Title").GetComponent<TMP_Text> ().text = languageManager.GetString("control_"+input.name.ToLower());
		}
	}
}
