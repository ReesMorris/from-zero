using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuButton : MonoBehaviour {

	public enum PauseTypes {accept, decline};
	public PauseTypes pauseType;
	private PauseManager pauseManager;

	void Start() {
		GetComponent<Button>().onClick.AddListener(OnClick);
		pauseManager = GameObject.Find ("GameManager").GetComponent<PauseManager> ();
	}

	void OnClick() {
		switch (pauseType) {
		case PauseTypes.accept:
			pauseManager.OnModalAcceptClick ();
			break;
		case PauseTypes.decline:
			pauseManager.OnModalRejectClick ();
			break;
		}
	}

}
