using UnityEngine;
using UnityEngine.UI;

public class ChoiceButton : MonoBehaviour {

	private int value;

	void Start () {
		// Add the event listener (src: https://docs.unity3d.com/ScriptReference/UI.Button-onClick.html)
		Button btn = GetComponent<Button>();
		btn.onClick.AddListener(TaskOnClick);
	}

	// The event listener
	void TaskOnClick()
	{
		PlayerPrefs.SetInt ("playerChoice", value);
	}

	// Change the value
	public void SetValue(int newValue) {
		value = newValue;
	}
}
