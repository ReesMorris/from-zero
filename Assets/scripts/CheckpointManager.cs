using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckpointManager : MonoBehaviour {

	private StoryManager storyManager;

	void Start() {
		storyManager = GetComponent<StoryManager> ();

		Initialise ();
		RunCheckpoint ();
	}

	// Call this to set a new checkpoint
	public void SetCheckpoint(string name) {
		PlayerPrefs.SetString ("CurrentStory", name);
		PlayerPrefs.SetInt ("CurrentWorld", SceneManager.GetActiveScene ().buildIndex);
	}

	// Call this to reset the game back to the last saved checkpoint
	public void ResetToLastCheckpoint() {
		string name = PlayerPrefs.GetString ("CurrentStory");
		// The checkpoint exists and is in this world
		if (name != "") {
			// Tell the code that to skip any story that we've seen, and reload the level
			PlayerPrefs.SetInt ("QuickReset", 1);
			ReloadScene ();
		} else {
			PlayerPrefs.SetInt ("CurrentWorld", SceneManager.GetActiveScene ().buildIndex);
			ResetProgress ();
		}
	}

	// Resets the progress of the story and resets the scene
	public void ResetProgress() {
		PlayerPrefs.SetString ("CurrentStory", storyManager.firstStoryName);
		PlayerPrefs.SetInt ("CurrentWorld", SceneManager.GetActiveScene ().buildIndex);
		ReloadScene ();
	}

	// Defines necessary variables, should they not exist
	void Initialise() {
		// Our current scene isn't right, let's reset this scene
		if (PlayerPrefs.GetInt ("CurrentWorld") != SceneManager.GetActiveScene ().buildIndex) {
			PlayerPrefs.SetString ("CurrentStory", storyManager.firstStoryName);
		}

		PlayerPrefs.SetString("currentSceneName", SceneManager.GetActiveScene().name);
		if (PlayerPrefs.GetString ("CurrentStory") == "") {
			PlayerPrefs.SetString ("CurrentStory", storyManager.firstStoryName);
			PlayerPrefs.SetInt ("CurrentWorld", SceneManager.GetActiveScene ().buildIndex);
		}
	}

	// Runs the correct story that the player should be running
	void RunCheckpoint() {
		if (PlayerPrefs.GetInt ("CurrentWorld") == SceneManager.GetActiveScene ().buildIndex) {
			// Same scene
			string name = PlayerPrefs.GetString ("CurrentStory");
			storyManager.StartStory (name);
		} else {
			// Different scene
			string name = storyManager.firstStoryName;
			storyManager.StartStory (name);
		}
	}

	// Reloads the scene
	void ReloadScene() {
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}
}
