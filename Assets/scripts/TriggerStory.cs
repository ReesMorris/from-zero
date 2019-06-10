using UnityEngine;

public class TriggerStory : MonoBehaviour {

	public string storyName;

	private StoryManager storyManager;

	void Start() {
		storyManager = GameObject.Find ("GameManager").GetComponent<StoryManager> ();
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.transform.parent != null) {
			if (collider.transform.parent.name == "Player") {
				storyManager.StartStory (storyName);
				Destroy (this);
			}
		}
	}
}
