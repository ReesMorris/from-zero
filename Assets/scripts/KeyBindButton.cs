using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBindButton : MonoBehaviour {

	public string inputKey;
	public TMP_Text displayText;

	private KeyBindManager keyBindManager;

	void Start() {
		GameObject scene = GameObject.Find("Main Menu");
		if (scene != null) {
			// Main menu
			keyBindManager = GameObject.Find("Main Menu").GetComponent<KeyBindManager>();
		} else {
			// In game
			keyBindManager = GameObject.Find("GameManager").GetComponent<KeyBindManager>();
		}

		displayText.text = keyBindManager.GetDefaultKey (inputKey);

		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	void OnClick() {
		keyBindManager.StartBind (this);
	}
}
