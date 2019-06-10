using UnityEngine;

public class TriggerTutorial : MonoBehaviour {

	public string tutorialKey;
	public float duration;

	private TutorialManager tutorialManager;

	void Start() {
		tutorialManager = GameObject.Find ("GameManager").GetComponent<TutorialManager> ();
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.transform.parent != null) {
			if (collider.transform.parent.name == "Player") {
				tutorialManager.QueueTutorial (tutorialKey, duration);
				Destroy (this);
			}
		}
	}
}
