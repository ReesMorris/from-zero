using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {

	[Header("Logo")]
	public Image logo;

	[Header("Black Screen")]
	public float fadeSpeed = 0.05f;
	public GameObject blackScreen;
	public GameObject loading;

	[Header("Pause Menu")]
	public GameObject pauseMenu;

	[Header("Readables")]
	public GameObject readableContainer;
	public Image readable;
	private bool readableShowing = false;
	public bool ReadableShowing {
		get {
			return readableShowing;
		}
	}
	private float readableTime;

	private StoryManager storyManager;
	private InputManager inputManager;
	private List<string> menusOpen;
	private PauseManager pauseManager;

	void Start() {
		menusOpen = new List<string> ();
		storyManager = GetComponent<StoryManager> ();
		inputManager = GetComponent<InputManager> ();
		pauseManager = GetComponent<PauseManager> ();

		HideReadable ();
	}

	void Update() {
		// If user hides the readable
		if ((inputManager.InputMatchesKeyup ("UI_READABLE_HIDE")) || (readableShowing && readableTime + 0.01f < Time.time && inputManager.InputMatchesKeyup ("INVENTORY_PICK"))) {
			HideReadable ();
		}
	}

	// Call to show the black screen over the UI
	public void ShowBlackScreen() {
		loading.SetActive (false);
		blackScreen.GetComponent<Image> ().color = new Color(0, 0, 0, 1);
		blackScreen.SetActive (true);
	}

	// Show black screen with loading spinner
	public void ShowBlackLoadingScreen() {
		ShowBlackScreen ();
		loading.SetActive (true);

	}

	// Call to hide the black screen UI
	public void HideBlackScreen() {
		loading.SetActive (false);
		blackScreen.SetActive (false);
	}

	// Call to fade in from a black screen
	public void FadeOut(float time) {
		// Only fade if we're not skipping the cutscene, prevents bugs
		if (storyManager.Skipping) {
			HideBlackScreen ();
		} else {
			blackScreen.SetActive (true);
			StartCoroutine (Fade(time));
		}
	}

	// Call to fade in from a black screen
	public void FadeIn(float time) {
		blackScreen.SetActive (true);
		StartCoroutine (FadeInRoutine(time));
	}

	// Will fade the black screen to transparent
	IEnumerator Fade(float time) {
		Color colour;
		float alpha = 1;
		float speed = fadeSpeed / time;
		while(alpha > 0) {
			alpha -= speed;
			colour = blackScreen.GetComponent<Image> ().color;
			blackScreen.GetComponent<Image> ().color = new Color(0, 0, 0, alpha);
			yield return new WaitForSeconds(fadeSpeed);
		}
	}

	// Will fade the black screen to black
	IEnumerator FadeInRoutine(float time) {
		Color colour;
		float alpha = 0;
		float speed = fadeSpeed / time;
		while(alpha < 1) {
			alpha += speed;
			colour = blackScreen.GetComponent<Image> ().color;
			blackScreen.GetComponent<Image> ().color = new Color(0, 0, 0, alpha);
			yield return new WaitForSeconds(fadeSpeed);
		}
	}

	// Show a readable object
	public void ShowReadable(Readable item) {
		if (!pauseManager.IsPaused) {
			readableShowing = true;

			// Make it visible (so we can modify it)
			readableContainer.SetActive (true);

			// Add the image
			readable.sprite = item.sprite;

			// Specify the dimensions
			RectTransform rt = readableContainer.GetComponent<RectTransform> ();
			rt.sizeDelta = item.size;

			readableTime = Time.time;
			AddOpenMenuItem ("readable");
		}
	}

	public void HideReadable() {
		readableContainer.SetActive (false);
		readableShowing = false;
		RemoveOpenMenuItem ("readable");
	}

	public void ShowLogo() {
		logo.gameObject.SetActive (true);
	}

	public void HideLogo() {
		logo.gameObject.SetActive (false);
	}

	public void FadeLogo(float time) {
		StartCoroutine (StartLogoFade (time));
	}

	IEnumerator StartLogoFade(float time) {
		Color colour;
		float alpha = 1;
		float speed = fadeSpeed / time;
		do {
			alpha -= speed;
			colour = logo.color;
			logo.color = new Color(logo.color.r, logo.color.g, logo.color.b, alpha);
			yield return new WaitForSeconds(fadeSpeed);
		} while(colour.a > 0);
	}

	public bool MenuIsOpen() {
		return menusOpen.Count > 0;
	}

	void AddOpenMenuItem(string name) {
		menusOpen.Add (name);
	}

	void RemoveOpenMenuItem(string name) {
		menusOpen.Remove (name);
	}

	public void ShowGameObject(GameObject item) {
		AddOpenMenuItem (item.name);
		item.SetActive (true);
	}
	public void HideGameObject(GameObject item) {
		RemoveOpenMenuItem (item.name);
		item.SetActive (false);
	}
}
