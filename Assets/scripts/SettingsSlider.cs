using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsSlider : MonoBehaviour {

	public enum types {Music, Dialogue, SFX, Master};
	public types type;
	public Slider slider;
	public TMP_Text displayText;

	private SoundManager soundManager;

	void Start() {
		slider.onValueChanged.AddListener(delegate {OnChange(); });

		Init ();
		OnLoad ();
	}

	void Init() {
		GameObject scene = GameObject.Find("Main Menu");
		if (scene != null) {
			// Main menu
			soundManager = scene.GetComponent<SoundManager>();
		} else {
			// In game
			soundManager = GameObject.Find("GameManager").GetComponent<SoundManager>();
		}
	}

	// Called to set pre-defined values on load
	void OnLoad() {
		// Changing between sound volumes?
		if (type == types.Master) {
			slider.value = PlayerPrefs.GetFloat ("master_volume", 0f);
		} else if (type == types.Music) {
			slider.value = PlayerPrefs.GetFloat ("music_volume", 1f);
		} else if (type == types.SFX) {
			slider.value = PlayerPrefs.GetFloat ("sfx_volume", 1f);
		} else if (type == types.Dialogue) {
			slider.value = PlayerPrefs.GetFloat ("dialogue_volume", 1f);
		}
	}

	// Called when a button is changed
	void OnChange() {
		// Changing between sound volumes?
		if (type == types.Master) {
			PlayerPrefs.SetFloat ("master_volume", slider.value);
			print ("setting master_volume to " + slider.value);
			if (soundManager.volumeChanged != null) {
				soundManager.volumeChanged ();
			}
		} else if (type == types.Music) {
			PlayerPrefs.SetFloat ("music_volume", slider.value);
			if (soundManager.volumeChanged != null) {
				soundManager.volumeChanged ();
			}
		} else if (type == types.SFX) {
			PlayerPrefs.SetFloat ("sfx_volume", slider.value);
			if (soundManager.volumeChanged != null) {
				soundManager.volumeChanged ();
			}
		} else if (type == types.Dialogue) {
			PlayerPrefs.SetFloat ("dialogue_volume", slider.value);
			if (soundManager.volumeChanged != null) {
				soundManager.volumeChanged ();
			}
		}
	}
}
