using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

	private CutsceneManager cutsceneManager;
	private InputManager inputManager;
	private Inventory inventory;
	private CheckpointManager checkpointManager;
	private ConsoleManager consoleManager;
	private TutorialManager tutorialManager;

	private bool noclip;
	public bool Noclip {
		get {
			return noclip;
		}
		set {
			noclip = value;
		}
	}

	void Start() {
		cutsceneManager = GameObject.Find ("GameManager").GetComponent<CutsceneManager> ();
		inputManager = GameObject.Find ("GameManager").GetComponent<InputManager> ();
		checkpointManager = GameObject.Find ("GameManager").GetComponent<CheckpointManager> ();
		consoleManager = GameObject.Find ("GameManager").GetComponent<ConsoleManager> ();
		tutorialManager = GameObject.Find ("GameManager").GetComponent<TutorialManager> ();
		inventory = GetComponent<Inventory> ();

		if (PlayerPrefs.GetInt ("playerDied") == 1 && PlayerPrefs.GetInt ("deathTutShown") == 0) {
			tutorialManager.QueueTutorial ("tutorial_text_death_1", 3f);
			tutorialManager.QueueTutorial ("tutorial_text_death_2", 3f);
			PlayerPrefs.SetInt ("deathTutShown", 1);
		}

		PlayerPrefs.SetInt ("playerDied", 0);
	}
	
	void Update () {
		// If player is trying to use their weapon
		if (inputManager.InputMatches ("WEAPON_FIRE")) {
			Pickup pickup = GetComponent<Entity> ().holding; // we need this here rather than setup, incase the weapon changes in game
			// A weapon is equipped
			if (pickup != null) {
				// If no cutscene is currently playing, shoot the gun
				if (!cutsceneManager.CutsceneActive) {
					// This function will handle managing the specific weapon type; we don't care what it is in this script
					pickup.Use ();
				}
			}
		}

		// If player is trying to drop their weapon
		if (inputManager.InputMatchesKeyup ("INVENTORY_DROP")) {
			if (!cutsceneManager.CutsceneActive && !consoleManager.ConsoleOpen) {
				inventory.DropCurrentItem (false);
			}
		}

		// If noclip is enabled
		if (noclip) {
			transform.position = new Vector3 (transform.position.x, 1.1f, transform.position.z);
		}
	}

	// Called when the player dies
	public void OnDeath() {
		print ("Player died... resetting to last checkpoint");
		PlayerPrefs.SetInt ("playerDied", 1);

		// Empty the inventory, but preserve the pistol
		GameObject pistol = inventory.PreservePistol();
		inventory.ClearInventory ();
		if (pistol != null) {
			inventory.AddItemToInventory (pistol);
		}

		if (checkpointManager == null) {
			// We're most likely on a debug map, so let's just reload the scene
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		} else {
			checkpointManager.ResetToLastCheckpoint ();
		}
	}
}
