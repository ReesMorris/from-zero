using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsToggle : MonoBehaviour {

	public enum types {Language, Resolution, Fullscreen, QualityLevel, Movement};
	public types type;
	public bool translateOptions = true;
	public string[] options;
	public Button leftButton;
	public Button rightButton;
	public TMP_Text displayText;

	private int currentIndex = 0;
	private LanguageManager languageManager;
    private SettingsManager settingsManager;

	void Start () {
		Init ();

		leftButton.onClick.AddListener(CycleLeft);
		rightButton.onClick.AddListener(CycleRight);
	}

	void Init() {
		GameObject scene = GameObject.Find("Main Menu");
		if (scene != null) {
			// Main menu
            languageManager = scene.GetComponent<LanguageManager>();
            settingsManager = scene.GetComponent<SettingsManager>();
		} else {
			// In game
            languageManager = GameObject.Find("GameManager").GetComponent<LanguageManager>();
            settingsManager = GameObject.Find("GameManager").GetComponent<SettingsManager>();
		}

		languageManager.languageChanged += OnLanguageChanged; // update our text if the language is changed! :-)

		OnLoad ();
	}

	void OnLanguageChanged() {
		if (translateOptions) {
			displayText.text = languageManager.GetString (options [currentIndex]);
		}
	}

	void CycleLeft() {
		if ((currentIndex - 1) < 0) {
			currentIndex = options.Length - 1;
		} else {
			currentIndex--;
		}

		SetIndex (currentIndex);
	}

	void CycleRight() {
		if ((currentIndex + 1) > (options.Length - 1)) {
			currentIndex = 0;
		} else {
			currentIndex++;
		}

		SetIndex (currentIndex);
	}

	// Does not do any checking to see if the index exists!
	void SetIndex(int newIndex) {
		if (languageManager == null) {
			Init ();
		}
		currentIndex = newIndex;

		// set option text
		if (translateOptions) {
			displayText.text = languageManager.GetString (options [currentIndex]);
		} else {
			displayText.text = options [currentIndex];
		}

		OnChange ();
	}

	// Called to set pre-defined values on load
	void OnLoad() {
		// Changing between languages?
		if (type == types.Language) {
			switch (languageManager.language.ToString ()) {
			case "English":
				SetIndex (0);
				break;
			case "Italian":
				SetIndex (1);
				break;
			}
		}

		// Fullscreen
        if (type == types.Fullscreen && !settingsManager.forceFullscreen) {
            SetIndex(Screen.fullScreen ? 1 : 0);
		}

		// Resolution
        if (type == types.Resolution && !settingsManager.forceResolution) {
            SetIndex(PlayerPrefs.GetInt("resolution_index", 0)); // default: 800x600
		}

		// Quality level
		if (type == types.QualityLevel) {
			SetIndex (PlayerPrefs.GetInt ("quality_level", 4)); // default: high
		}

        // Movement type
		if (type == types.Movement) {
			SetIndex (PlayerPrefs.GetInt ("movement_control_scheme", 0));
		}
	}

	// Called when a button is changed
	void OnChange() {
		// Changing between languages?
		if (type == types.Language) {
			switch (currentIndex) {
			case 0:
			default:
				languageManager.ChangeLanguage (LanguageManager.languages.English);
				break;
			case 1:
				languageManager.ChangeLanguage (LanguageManager.languages.Italian);
				break;
			}
		}

		// Changing between fullscreen and not?
		if (type == types.Fullscreen) {
			if (currentIndex == 0) {
				Screen.fullScreen = false;
			} else {
				Screen.fullScreen = true;
			}
		}

		// Changing screen resolution?
		if (type == types.Resolution) {
			PlayerPrefs.SetInt ("resolution_index", currentIndex);
			string[] wh = options [currentIndex].Split ('x');
			int w = int.Parse(wh [0]);
			int h = int.Parse(wh [1]);
			PlayerPrefs.SetInt ("resolution_w", w);
			PlayerPrefs.SetInt ("resolution_h", h);

			Screen.SetResolution (w, h, Screen.fullScreen);
		}

		// Changing quality level?
		if (type == types.QualityLevel) {
			QualitySettings.SetQualityLevel (currentIndex);
			PlayerPrefs.SetInt ("quality_level", currentIndex);
		}

		// Changing control scheme?
		if (type == types.Movement) {
			PlayerPrefs.SetInt ("movement_control_scheme", currentIndex);
		}
	}
}
