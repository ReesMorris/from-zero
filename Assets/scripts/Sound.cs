using UnityEngine;

public class Sound : MonoBehaviour {

	[Header("Generic")]
	public int type;
	public bool destroyOnComplete = false;
	public bool use3dSound = false;

	[Range(1, 100)]
	public int areaVolume;

	[Header("Subtitles")]
	public bool showSubtitles;
	public Subtitle[] subtitles;

	private AudioSource audioSource;
	private SoundManager soundManager;

	void Start() {
		Init ();
		audioSource = GetComponent<AudioSource> ();
		if (use3dSound) {

			audioSource.spatialBlend = 1f;
			audioSource.dopplerLevel = 0;
			audioSource.rolloffMode = AudioRolloffMode.Linear;

			UpdateAreaVolume ();
		}

		soundManager.volumeChanged += UpdateSoundVolume; // update our sound as the volume is changed
		UpdateSoundVolume();
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

	void Update() {
		//UpdateAreaVolume ();

		if (destroyOnComplete) {
			if (audioSource.time >= audioSource.clip.length) {
				Destroy (gameObject);
			}
		}

		audioSource.pitch = Time.timeScale; // change pitch based on speed
	}

	// Gets the min/max based on the current volume
	void UpdateAreaVolume() {
		float min = (areaVolume * 0.1f)  + 1;
		float max = (min * Mathf.Log(min * 3f)) + 1;

		audioSource.minDistance = min;
		audioSource.maxDistance = max;
	}

	void UpdateSoundVolume() {
		switch (type) {
		case 0: // dialogue
			audioSource.volume = PlayerPrefs.GetFloat ("dialogue_volume");
			break;
		case 1: // sfx
			audioSource.volume = PlayerPrefs.GetFloat ("sfx_volume");
			break;
		case 2: // music
			audioSource.volume = PlayerPrefs.GetFloat ("music_volume");
			break;
		}
		audioSource.volume -= PlayerPrefs.GetFloat ("master_volume");
	}

}