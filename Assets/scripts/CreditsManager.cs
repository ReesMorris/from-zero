using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour {

	[Header("Objects")]
	public AudioSource music;
	public Image logo;
	public RectTransform creditsContainer;
	public Image black;

	[Header("Config")]
	public float moveDelay;
	public float moveSpeed;
	public float endOffset;

	private bool ending = false;

	void Start () {
		StartCoroutine ("FadeLogo");
	}

	void Update() {
		if (creditsContainer.offsetMax.y >= endOffset || Input.anyKey) {
			if (!ending) {
				// allow ending
				ending = true;
				StartCoroutine ("BackToMainMenu");
			}
		}
	}

	IEnumerator FadeLogo() {
		logo.color = new Color (255, 255, 255, 0f);

		StartCoroutine ("Cutscene");

		while (logo.color.a < 255) {
			logo.color = new Color (255, 255, 255, logo.color.a + 0.01f);
			yield return new WaitForSeconds (0.05f);
		}
	}

	IEnumerator Cutscene() {
		yield return new WaitForSeconds(moveDelay);

		while (true) {
			creditsContainer.offsetMax = new Vector2 (0f, creditsContainer.offsetMax.y + moveSpeed);
			yield return new WaitForSeconds (0.001f);
		}
	}

	IEnumerator BackToMainMenu() {
		StartCoroutine ("FadeBlack");
		while (music.volume > 0) {
			music.volume -= 0.05f;
			yield return new WaitForSeconds (0.1f);
		}
		yield return new WaitForSeconds (1f);

		if(PlayerPrefs.GetInt ("loadToResults") == 0)
			SceneManager.LoadScene ("(UI) Main Menu");
		else
			SceneManager.LoadScene ("(UI) Choices");
	}

	IEnumerator FadeBlack() {
		while (black.color.a < 255) {
			black.color = new Color (0, 0, 0, black.color.a + 0.05f);
			yield return new WaitForSeconds (0.1f);
		}
	}
}
