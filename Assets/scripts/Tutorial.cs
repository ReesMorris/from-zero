using UnityEngine;

[System.Serializable] // [5]
public class Tutorial {

	public string text;
	public float duration;

	public Tutorial(string _text, float _duration) {
		text = _text;
		duration = _duration;
	}
}
