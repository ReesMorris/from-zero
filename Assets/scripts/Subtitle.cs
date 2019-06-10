using UnityEngine;

[System.Serializable] // [5]
public class Subtitle {

	public string key;
	public float startDelay;
	public float duration;

	private float startTime;
	public float StartTime {
		get {
			return startTime;
		}
		set {
			startTime = value;
		}
	}

	private float endTime;
	public float EndTime {
		get {
			return endTime;
		}
		set {
			endTime = value;
		}
	}
}
