using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public RectTransform text;

	private Button button;

	void Start() {
		button = GetComponent<Button> ();
	}

	//src: https://gamedev.stackexchange.com/questions/108625/how-to-detect-mouse-over-for-ui-image-in-unity-5 10 feb 2018
	public void OnPointerEnter(PointerEventData eventData) {
		if (button.interactable) {
			StartCoroutine ("MoveButtonRight");
		}
	}

	public void OnPointerExit(PointerEventData eventData) {
		StopCoroutine ("MoveButtonRight");
		StartCoroutine ("MoveButtonLeft");
	}

	IEnumerator MoveButtonRight() {
		while (text.anchoredPosition.x < 15f) {
			text.anchoredPosition = new Vector2 (text.anchoredPosition.x + 1f, 0f);
			yield return new WaitForSeconds (0.005f);
		}
	}

	IEnumerator MoveButtonLeft() {
		while (text.anchoredPosition.x > 0f) {
			text.anchoredPosition = new Vector2 (text.anchoredPosition.x - 1f, 0f);
			yield return new WaitForSeconds (0.001f);
		}
	}
}
