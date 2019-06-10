using UnityEngine;

public class StoryTriggerable : MonoBehaviour {

	public string storyName;

	private StoryManager storyManager;

	void Start() {
		storyManager = GameObject.Find ("GameManager").GetComponent<StoryManager> ();
	}

	// Called by InventorySearcher when F (default) is pressed
	public void OnTrigger() {
		storyManager.StartStory (storyName);
	}
}
