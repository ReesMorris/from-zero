using UnityEngine;

[System.Serializable] // [5]
public class SoundTypes {

	public string name;
	public GameObject directory;
	public bool stopOthers;
	[Range(0f, 1f)] public float volume = 1f;
}
