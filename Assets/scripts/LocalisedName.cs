using UnityEngine;

public class LocalisedName : MonoBehaviour {

	public string key;

	private LanguageManager languageManager;

	void Start () {
		Init ();

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

	void SetText() {
		gameObject.name = languageManager.GetString (key);
	}

	public void UpdateText(string newKey) {
		key = newKey;
		SetText ();
	}
}
