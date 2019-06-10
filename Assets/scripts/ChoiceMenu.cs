using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class ChoiceMenu : MonoBehaviour {
	public string choiceURL;
	public AudioSource transitionSFX;
	public Image black;
	public GameObject bottomBar;
	public GameObject loadingSymbol;
	public GameObject loadingText;

	private SoundManager soundManager;
	private bool transitioning;
	private int waitingToLoad;

	void Start() {
		waitingToLoad = 0;
		transitioning = false;
		soundManager = GetComponent<SoundManager> ();
	}

	void Update() {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			TransitionToMainMenu ();
		}
	}

	public void TransitionToMainMenu() {
		if (!transitioning) {
			transitioning = true;
			bottomBar.SetActive (false);
			StartCoroutine ("MainMenu");
		}
	}

	IEnumerator MainMenu() {
		StartCoroutine ("FadeBlack");
		soundManager.PlaySound (transitionSFX, 0);
		yield return new WaitForSeconds (1f);
		PlayerPrefs.SetInt ("uploadResults", 0);
		SceneManager.LoadScene ("(UI) Main Menu");
	}

	IEnumerator FadeBlack() {
		while (black.color.a < 255) {
			black.color = new Color (0, 0, 0, black.color.a + 0.1f);
			yield return new WaitForSeconds (0.08f);
		}
	}

	public void StartingLoad() {
		waitingToLoad++;
		StartCoroutine ("LoadSpeed");
		loadingSymbol.SetActive (true);
	}

	public void LoadingComplete() {
		waitingToLoad--;
		if (waitingToLoad <= 0) {
			loadingSymbol.SetActive (false);
			loadingText.SetActive (false);
		}
	}

	IEnumerator LoadSpeed() {
		yield return new WaitForSeconds (5f);
		if (waitingToLoad > 0)
			loadingText.SetActive (true);
	}
}