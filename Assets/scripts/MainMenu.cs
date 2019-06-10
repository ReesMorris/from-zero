using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour {

	public string version;
	public Edition edition;
	public AudioSource transition;
	public AudioSource music;
	public Image black;
	public Animator UI;
	public GameObject bottomBar;
	public GameObject resetButton;
	public GameObject episodes;

	[Header("Scenes")]
	public string firstScene;
	public string firstStory;

	[Header("Default Menu")]
	public VideoPlayer video;
	public TMP_Text playText;

	[Header("Settings")]
	public GameObject settingsMenu;
	public GameObject[] menus;
	public TMP_Text titleText;

	[Header("Editions")]
	public TMP_Text buildVersion;

	[Header("Episodes")]
	public Episodes[] allEpisodes;
	public TMP_Text episodeNo;
	public TMP_Text episodeTitle;
	public TMP_Text episodeDesc;
	public TMP_Text episodePlay;
	public Image episodeBackground;
	public GameObject confirmation;
	public GameObject confirmation2;
	public GameObject[] episodeButtons;
	public GameObject selected;

	private LanguageManager lm;
	private KeyBindManager kbm;
	private bool transitioning = false;
	private RectTransform currentButton;
	private string menu = "";
	private int selectedEpisode = 0;
	private EditionManager editionManager;

	[Header("Scenes")]
	public string creditScene;

	void Start() {
		// Set the default settings
		settingsMenu.SetActive(true);
		SettingsMenus(true);

		Cursor.visible = true;
		Time.timeScale = 1f; // if coming from pause screen
		lm = GetComponent<LanguageManager>();
		kbm = GetComponent<KeyBindManager>();
		editionManager = GetComponent<EditionManager>();

		bottomBar.SetActive(false);
		episodes.SetActive(false);
		confirmation.SetActive(false);
		confirmation2.SetActive(false);

		lm.languageChanged += UpdatePlayButton;

		SetEdition();
		SetResolution();
		ManageButtons();

		// If coming from a game scene
		if (PlayerPrefs.GetInt("showSettings") == 1) {
			Settings();
		}
	}

	void Update() {
		if (Input.GetKey(KeyCode.Escape)) {
			if (!kbm.IsBinding && menu != "") {
				MainScreen();
			}
		} else if (Input.GetKey(KeyCode.X) && menu == "episodes") {
			ResetStoryProgress(false);
		}
	}

	void SetResolution() {
		if (editionManager.Is(Edition.DigitalDownload)) {
			int w = PlayerPrefs.GetInt("resolution_w", 800);
			int h = PlayerPrefs.GetInt("resolution_h", 600);

			Screen.SetResolution(w, h, Screen.fullScreen);
		}
	}

	void SettingsMenus(bool active) {
		foreach (GameObject go in menus) {
			go.SetActive(active);
		}
		titleText.text = "";
	}

	void SetEdition() {
		editionManager.SetEdition(edition);
		buildVersion.text = edition.ToString().ToLower() + "-" + version;
	}

	void ManageButtons() {
		UpdatePlayButton();
	}

	public void MainScreen() {
		// Make the button click stop bindings
		if (menu == "settings" && kbm.IsBinding) {
			kbm.StopBinding();
		} else if (CanTransition()) {
			FadeIn();
			transitioning = false;
			if (menu == "episodes") {
				UI.Play("main_menu_episodes_out");
			}
			menu = "";
		}
	}

	void OnLanguageChanged() {
		UpdatePlayButton();
	}

	public void Credits() {
		if (CanTransition()) {
			StartCoroutine("StartCredits");
		}
	}

	public void Quit() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void Settings() {
		if (CanTransition()) {
			StartCoroutine("StartSettings");
		}
	}

	public void Episodes() {
		if (CanTransition()) {
			confirmation.SetActive(false);
			confirmation2.SetActive(false);
			episodes.SetActive(true);
			SetEpisode(0);
			SetBlockedEpisodes();
			FadeOut(true);
			transitioning = false;
			menu = "episodes";
			UI.Play("main_menu_episodes_in");
		}
	}

	void SetBlockedEpisodes() {
		int max = PlayerPrefs.GetInt("currentEpisodeNumber");
		SetEpisode(max);
		for (int i = 1; i < episodeButtons.Length; i++) {
			if (i > max) {
				episodeButtons[i].GetComponent<Button>().interactable = false;
				episodeButtons[i].transform.Find("Label").GetComponent<TMP_Text>().color = new Color(1, 1, 1, 0.75f);
				episodeButtons[i].transform.Find("Image").GetComponent<Image>().sprite = allEpisodes[i].episodeImageBlur;
			}
		}
	}

	void UpdatePlayButton() {
		if (PlayerPrefs.GetInt("currentEpisodeNumber") == 0 && PlayerPrefs.GetString("CurrentStory") == firstStory) {
			playText.text = lm.GetString("ui_title_episodes_new");
		} else {
			playText.text = lm.GetString("ui_title_episodes_continue");
		}
	}

	public void SetEpisode(int index) {
		if (AllModalsClosed()) {
			Episodes e = allEpisodes[index];
			episodeNo.text = "Episode " + (index + 1);
			episodeTitle.text = lm.GetString("episode_" + (index + 1) + "_title");
			episodeDesc.text = lm.GetString("episode_" + (index + 1) + "_desc");
			if (PlayerPrefs.GetInt("currentEpisodeNumber") == index) {
				episodePlay.text = string.Format(lm.GetString("ui_title_continue_episode"), (index + 1));
			} else {
				episodePlay.text = string.Format(lm.GetString("ui_title_play_episode"), (index + 1));
			}
			episodeBackground.sprite = e.episodeImage;
			selectedEpisode = index;

			selected.transform.SetParent(episodeButtons[index].transform.Find("Image"), false);
			RectTransform rt = selected.GetComponent<RectTransform>();
			rt.offsetMax = new Vector2(5f, 5f);
			rt.offsetMin = new Vector2(-5f, -5f);
		}
	}

	bool AllModalsClosed() {
		return !confirmation.activeSelf && !confirmation2.activeSelf;
	}

	public void ResetStoryProgress(bool skipCheck) {
		confirmation.SetActive(false);
		if (!skipCheck) {
			confirmation2.SetActive(true);
		} else {
			PlayerPrefs.SetString("CurrentStory", "");
			PlayerPrefs.SetString("currentSceneName", allEpisodes[selectedEpisode].firstSceneName);
			StartCoroutine("PlayGame");
		}
	}

	public void PlayEpisodeButton(bool skipCheck) {
		confirmation2.SetActive(false);
		if (PlayerPrefs.GetInt("currentEpisodeNumber") > selectedEpisode && !skipCheck) {
			confirmation.SetActive(true);
			//UI.Play ("main_menu_reset_progress_in");
		} else {
			if (CanTransition()) {
				// check to see if the user is changing their episode
				if (episodes.activeSelf) {
					// they are, so let's see if they're changing the episode or just trying to play it again
					if (PlayerPrefs.GetInt("currentEpisodeNumber") != selectedEpisode) {
						PlayerPrefs.SetString("currentSceneName", allEpisodes[selectedEpisode].firstSceneName);
						PlayerPrefs.SetInt("currentEpisodeNumber", 0);
					}
				}
				StartCoroutine("PlayGame");
			}
		}
	}

	public void CloseConfirmation() {
		confirmation.SetActive(false);
		confirmation2.SetActive(false);
	}

	bool CanTransition() {
		if (!transitioning) {
			transitioning = true;
			transition.Play();
			return true;
		}
		return false;
	}

	void FadeOut(bool showBottomBar) {
		UI.StopPlayback();
		UI.Play("main_menu_default_sidebar_fadeout");
		if (showBottomBar) {
			bottomBar.SetActive(true);
			UI.Play("main_menu_bottom_bar_fadein");
		}
		//video.Pause ();
	}

	void FadeIn() {
		UI.Play("main_menu_default_sidebar_fadein");
		if (menu != "settings") {
			UI.Play("main_menu_bottom_bar_fadeout");
		} else {
			UI.Play("main_menu_esc_fadeout");
		}
		//video.Play ();
	}

	IEnumerator StartCredits() {
		StartCoroutine("FadeMusic");
		StartCoroutine("FadeBlack");
		FadeOut(false);
		yield return new WaitForSeconds(1f);
		PlayerPrefs.SetInt("loadToResults", 0);
		SceneManager.LoadScene(creditScene);
	}

	IEnumerator FadeMusic() {
		while (music.volume > 0) {
			music.volume -= 0.05f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	IEnumerator FadeBlack() {
		while (black.color.a < 255) {
			black.color = new Color(0, 0, 0, black.color.a + 0.1f);
			yield return new WaitForSeconds(0.08f);
		}
	}

	IEnumerator PlayGame() {
		StartCoroutine("FadeMusic");
		StartCoroutine("FadeBlack");
		UI.Play("main_menu_bottom_bar_fadeout");
		yield return new WaitForSeconds(1f);
		PlayerPrefs.SetInt("QuickReset", 0); // we want to see cutscenes again incase we forgot

		if (PlayerPrefs.GetString("currentSceneName") == "") {
			SceneManager.LoadScene(firstScene);
		} else {
			SceneManager.LoadScene(PlayerPrefs.GetString("currentSceneName"));
		}
	}

	public void ToggleSettingMenu(int index) {
		HideSettingsMenus();
		menus[index].SetActive(true);
		if (index == 0) {
			menus[index].GetComponent<SettingsControls>().SetupPanel();
		}
	}

	void HideSettingsMenus() {
		for (int i = 0; i < menus.Length; i++) {
			menus[i].SetActive(false);
		}
	}

	IEnumerator StartSettings() {
		SettingsMenus(false);
		PlayerPrefs.SetInt("showSettings", 0);
		transitioning = true;
		UI.Play("main_menu_default_sidebar_fadeout");
		yield return new WaitForSeconds(0.41f);
		menu = "settings";
		UI.Play("main_menu_settings_in");
		bottomBar.SetActive(true);
		UI.Play("main_menu_esc_fadein");
		yield return new WaitForSeconds(0.3f);
		transitioning = false;
	}

	public void SetSettingsTitleText(LocalisedText key) {
		LocalisedText lt = titleText.GetComponent<LocalisedText>();
		lt.key = key.key;
		lt.SetText();
	}

}