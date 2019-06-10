using UnityEngine;

public class Story_DamageTrigger : MonoBehaviour {

	private GameObject gm;
	private TutorialManager tm;
	private StoryManager sm;

	void Start() {
		gm = GameObject.Find ("GameManager");
		tm = gm.GetComponent<TutorialManager> ();
		sm = gm.GetComponent<StoryManager> ();
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.name == "Body_Parts") {
			if (collider.transform.parent.name == "Player") {
				sm.StartStory ("Story_Fuel_Station_2");
				tm.QueueTutorial ("fuel_station_tutorial_easter", 3f);
			}
		}
	}

}
