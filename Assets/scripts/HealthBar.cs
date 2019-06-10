using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

	private CameraController cameraController;
	private CutsceneManager cutsceneManager;
	private Image healthBarFill;
	private GameObject child;
	public bool showAlways;
	private bool shouldShow;

	void Start () {
		shouldShow = false;
		child = gameObject.transform.GetChild (0).gameObject;
		healthBarFill = gameObject.transform.GetChild (0).transform.GetChild (0).GetComponent<Image> ();
		cameraController = GameObject.Find ("GameManager").GetComponent<CameraController> ();
		cutsceneManager = GameObject.Find ("GameManager").GetComponent<CutsceneManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt (cameraController.mainCamera.transform);

		// Determine whether to show the health bar or not
		if (cutsceneManager.CutsceneActive) {
			// If a cutscene is playing, priority is to never show it
			HideDisplay();
		} else {
			// No cutscene, let's see what the game script wants
			if (shouldShow || showAlways) {
				ShowDisplay ();
			} else {
				HideDisplay ();
			}
		}
	}

	public void Hide() {
		shouldShow = false;
	}

	public void Show() {
		shouldShow = true;
	}

	public void SetFillAmount(float newFill) {
		healthBarFill.fillAmount = newFill;
	}

	public void Destroy() {
		Destroy (gameObject);
	}

	void HideDisplay() {
		child.SetActive (false);
	}

	void ShowDisplay() {
		child.SetActive (true);
	}
}
