using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

// This script is based off [3]
// This is a modified version of that file

public class LanguageManager : MonoBehaviour {

	[Header("Language")]
	[Tooltip("Must be in the resources folder, do not include file extention")]
	public string languageXML;
	public enum languages{English, Italian}; // [4]
	public languages language;

	private Lang LMan;
	private string currentLanguage;
	private InputManager inputManager;
	public delegate void OnLanguageChanged(); // let other scripts know that our language has been changed
	public OnLanguageChanged languageChanged;

	void Start () {
		SavedLanguage ();
		ChangeLanguage (language);
	}

	void SavedLanguage() {
		language = (languages)System.Enum.Parse( typeof(languages), PlayerPrefs.GetString("lang", "English") ); // https://forum.unity.com/threads/saving-player-prefs-with-enums.397304/ 4th march 2018
	}

	public string GetString(string name) {
		if (LMan == null) {
			SavedLanguage ();
			ChangeLanguage (language);
		}

		return StringReplacements(LMan.getString (name));
	}

	public void ChangeLanguage(languages newLanguage) {
		PlayerPrefs.SetString ("lang", newLanguage.ToString ());
		currentLanguage = newLanguage.ToString ();
		LMan = new Lang (languageXML, currentLanguage, false);
		if (languageChanged != null) {
			languageChanged ();
		}
	}

	string StringReplacements(string text) {
		if (inputManager == null) {
			GameObject gm = GameObject.Find ("GameManager");
			if (gm != null) {
				// game
				inputManager = gm.GetComponent<InputManager> ();
			} else {
				// main menu
				GetComponent<GameManager> ();
			}
		}
		
		// Line Breaks
		text = text.Replace ("~n~", "\n");

		// Sprites
		text = Regex.Replace(text, "~SPRITE_(\\d)*~", "<sprite=$1>");

		// Colours https://answers.unity.com/questions/248195/regular-expressions-in-unity.html
		text = Regex.Replace(text, "~s~", "<color=#FFF>"); // white
		text = Regex.Replace(text, "~r~", "<color=#cd2a2a>"); // red
		text = Regex.Replace(text, "~b~", "<color=#2ab4cd>"); // blue
		text = Regex.Replace(text, "~c~", "<color=#ababab>"); // gray
		text = Regex.Replace(text, "~g~", "<color=#63cd2a>"); // green
		text = Regex.Replace(text, "~y~", "<color=#cdc72a>"); // yellow

		// Replace input key with keycode
		Match m = Regex.Match(text, "(?<key>~KEY_(.*)~)"); // src: https://stackoverflow.com/questions/628556/returning-only-part-of-match-from-regular-expression
		string key = m.Groups["key"].Value;
		if (key != "") {
			// We have a key to replace!

			// First, let's format it to get rid of the ~KEY_  ... ~ parts  (sources from 24 Jan 2018)
			key = key.Trim('~'); // src: https://stackoverflow.com/questions/42964150/how-to-remove-first-and-last-character-of-a-string-in-c
			key = key.Substring(4, key.Length - 4); // src: https://stackoverflow.com/questions/7186648/how-to-remove-first-10-characters-from-a-string

			// We now have just the key value, so we can do a replacement
			text = Regex.Replace(text, "~KEY_(.*)~", inputManager.GetKeyValue(key));		
		}

		return text;
	}
}
