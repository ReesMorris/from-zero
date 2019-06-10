using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class TriggerLoadLevel : MonoBehaviour {

	public bool showBlackScreen;
	public string sceneName;
	public float delay;

	private UIManager uiManager;

	void Start() {
		uiManager = GameObject.Find ("GameManager").GetComponent<UIManager> ();
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.transform.parent != null) {
			if (collider.transform.parent.name == "Player") {
				if (showBlackScreen)
					uiManager.ShowBlackLoadingScreen();
				StartCoroutine ("Load");
			}
		}
	}

	IEnumerator Load() {
		yield return new WaitForSeconds (delay);
		SceneManager.LoadScene (sceneName);
	}
}
