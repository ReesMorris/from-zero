using UnityEngine;

public class TriggerAudio : MonoBehaviour {

	public AudioSource audioClip;
	public float delay;
	public bool triggerOnce;

	private SoundManager sm;

	void Start() {
		sm = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
	}

	void OnTriggerEnter(Collider collider) {
		print ("trigger enter");
		sm.PlaySound (audioClip, delay);

		if (triggerOnce) {
			Destroy (this);
		}
	}
}
