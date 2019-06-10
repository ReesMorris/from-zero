using UnityEngine;
using TMPro;

public class LocalisedText : MonoBehaviour {

	public string key;

	private LanguageManager languageManager;
	private TMP_Text text;

	void Start () {
		Init ();

		text = GetComponent<TMP_Text> ();
		languageManager.languageChanged += SetText; // update our text if the language is changed! :-)
		SetText ();
	}

	void Init() {
		GameObject scene = GameObject.Find("Main Menu");
		if (scene != null) {
			// Main menu
			languageManager = GameObject.Find("Main Menu").GetComponent<LanguageManager>();
		} else {
			// In game
			languageManager = GameObject.Find("GameManager").GetComponent<LanguageManager>();
		}
	}

	public void SetText() {
		text.text = languageManager.GetString (key);
	}

	public void UpdateText(string newKey) {
		if (languageManager == null) {
			Start ();
		}

		key = newKey;
		text.text = languageManager.GetString (key);
	}

	public void TakeParameters(params string[] parameters) {
		text.text = string.Format(text.text, parameters);
	}
}
