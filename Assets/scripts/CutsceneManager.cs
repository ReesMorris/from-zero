using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour {

	public GameObject mainCamera;
	public GameObject cutsceneCamera;
	public GameObject audioListener;

	private UIManager uiManager;
	private ChoiceManager choiceManager;
	private CutsceneCamera cs;
	private PauseManager pauseManager;
	private bool cutsceneActive;
	public bool CutsceneActive {
		get {
			return cutsceneActive;
		}
	}

	void Start() {
		choiceManager = GetComponent<ChoiceManager> ();
		pauseManager = GetComponent<PauseManager> ();
	}

	void Update() {
		// Show or hide the cursor
		if (choiceManager.ChoicePending || !CutsceneActive || pauseManager.IsPaused) {
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		} else {
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	}

	// Public function to start a cutscene, taking a clip as a parameter
	public void PlayCutscene(string clipName) {
		StartCutscene ();
		cs.StartCutscene (clipName);
	}

	// Puts the game into a cutscene mode, changing the camera
	void StartCutscene() {
		mainCamera.SetActive (false);
		cutsceneCamera.SetActive (true);

		// The cutscene camera will be inactive from the start, so we can't easily get its
		// components. This ensures that we do.
		if (cs == null) {
			cs = cutsceneCamera.GetComponent<CutsceneCamera> ();
		}

		cutsceneActive = true;
		audioListener.SetActive (false);
		cutsceneActive = true;

		if (uiManager == null) {
			uiManager = GetComponent<UIManager> ();
		}
		uiManager.HideReadable ();
	}

	// Puts the game back into normal mode, changing the camera
	public void EndCutscene() {
		mainCamera.SetActive (true);
		cutsceneCamera.SetActive (false);
		cutsceneActive = false;
		audioListener.SetActive (true);
		cutsceneActive = false;
	}
}
