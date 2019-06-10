using UnityEngine;

public class SettingsManager : MonoBehaviour {

    [Header("Fullscreen")]
    public bool forceFullscreen;
    public bool isFullscreen;

    [Header("Resolution")]
    public bool forceResolution;
    public Vector2 resolution;

    [Header("Hidden Settings")]
    public GameObject[] hiddenSettings;

    private MainMenu mainMenu;

    void Start() {
        mainMenu = GetComponent<MainMenu>();

        HideEditionFeatures();
        FirstTimeSettings();
        DefaultSettings();
    }

    // Deactivate settings that are not meant for this edition
    void HideEditionFeatures() {
        foreach (GameObject go in hiddenSettings) {
            go.SetActive(false);
        }
    }

    // If playing for the first time, set some initial variables
    void FirstTimeSettings() {
        // Only run if this is the first time
        if (!PlayerPrefs.HasKey("settings_set")) {
            PlayerPrefs.SetInt("settings_set", 1);

            // Story
            PlayerPrefs.SetInt("currentEpisodeNumber", 0);
            PlayerPrefs.SetString("CurrentStory", mainMenu.firstStory);

            // Audio
            PlayerPrefs.SetFloat("master_volume", 0f);
            PlayerPrefs.SetFloat("music_volume", 1f);
            PlayerPrefs.SetFloat("sfx_volume", 1f);
            PlayerPrefs.SetFloat("dialogue_volume", 1f);

            // Gameplay
            PlayerPrefs.SetInt("movement_control_scheme", 0);
        }
    }

    // Set up default settings for the game
    void DefaultSettings() {
        // Fullscreen
        if(forceFullscreen) {
            Screen.fullScreen = isFullscreen;
        }

        // Screen resolution
        if (forceResolution) {
            Screen.SetResolution((int)resolution.x, (int)resolution.y, Screen.fullScreen);
        }
    }
}