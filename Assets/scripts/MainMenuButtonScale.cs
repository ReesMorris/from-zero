using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuButtonScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public RectTransform text;
	public float scaleIncrease;

	private Button button;
	private Vector3 originalScale;
	private float inc;

	void Start() {
		button = GetComponent<Button> ();
		inc = (scaleIncrease / 10f);
		originalScale = button.transform.localScale;
	}

	//src: https://gamedev.stackexchange.com/questions/108625/how-to-detect-mouse-over-for-ui-image-in-unity-5 10 feb 2018
	public void OnPointerEnter(PointerEventData eventData) {
		StopCoroutine ("ScaleDown");
		StartCoroutine ("ScaleUp");
	}

	public void OnPointerExit(PointerEventData eventData) {
		StopCoroutine ("ScaleUp");
		StartCoroutine ("ScaleDown");
	}

	IEnumerator ScaleUp() {
		while (button.transform.localScale.x < originalScale.x + scaleIncrease) {
			Vector3 currScale = button.transform.localScale;
			button.transform.localScale = new Vector3 (currScale.x + inc, currScale.y + inc, currScale.z + inc);
			yield return new WaitForSeconds (0.005f);
		}
	}

	IEnumerator ScaleDown() {
		while (button.transform.localScale.x > originalScale.x) {
			Vector3 currScale = button.transform.localScale;
			button.transform.localScale = new Vector3 (currScale.x - inc, currScale.y - inc, currScale.z - inc);
			yield return new WaitForSeconds (0.005f);
		}
	}
}
