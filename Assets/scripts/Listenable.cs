using UnityEngine;

public class Listenable : MonoBehaviour {

	public AudioSource audioSource;
	public bool destroyOnActivate;

	[Header("Keys")]
	public bool useCustomKeys;
	public string onText;
	public string offText;

	private LocalisedName localisedName;

	void Start() {
		localisedName = GetComponent<LocalisedName> ();
	}

	void Update() {
		if (useCustomKeys) {
			if (audioSource.isPlaying)
				localisedName.UpdateText (offText);
			else
				localisedName.UpdateText (onText);
		}
	}
}
