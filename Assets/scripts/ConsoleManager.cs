using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class ConsoleManager : MonoBehaviour {

	[Header("Config")]
	public float destroyGoodAfter = 3f;
	public float destroyBadAfter = 1f;

	[Header("Objects")]
	public GameObject consoleContainer;
	public TMP_InputField consoleInput;
	public GameObject feedbackContainer;
	public TMP_Text feedbackText;

	private bool feedbackGood;
	private GameObject player;
	private InputManager inputManager;
	private CheckpointManager checkpointManager;
	private bool consoleOpen;
	public bool ConsoleOpen {
		get {
			return consoleOpen;
		}
	}

	private Entity playerEntity;
	private StoryManager storyManager;
	private List<string> prevEntries;
	private int prevIndex;

	void Start() {
		prevEntries = new List<string> ();
		prevEntries.Insert (0, "");
		prevIndex = 0;
		player = GameObject.Find ("Player").gameObject;
		playerEntity = player.GetComponent<Entity> ();
		inputManager = GetComponent<InputManager> ();
		storyManager = GetComponent<StoryManager> ();
		checkpointManager = GetComponent<CheckpointManager> ();

		CloseConsole ();
		CloseFeedbackContainer ();
	}

	void Update () {
		if (inputManager.InputMatchesKeyup ("CONSOLE_OPEN")) {
			OpenConsole ();
		}
		if (inputManager.InputMatchesKeyup ("CONSOLE_CLOSE")) {
			CloseConsole ();
		}
		if (inputManager.InputMatchesKeyup ("CONSOLE_SUBMIT")) {
			ProcessConsoleInput (consoleInput.text);
		}
		if (inputManager.InputMatchesKeyup ("CONSOLE_CYCLE_UP")) {
			CycleConsoleUp ();
		}
		if (inputManager.InputMatchesKeyup ("CONSOLE_CYCLE_DOWN")) {
			CycleConsoleDown ();
		}
	}

	// Cycle console text up
	void CycleConsoleUp () {
		if(prevEntries.Count > prevIndex + 1) {
			prevIndex++;
			consoleInput.text = prevEntries [prevIndex];
		}
		MoveCaretToEnd ();
	}

	// Cycle console text down
	void CycleConsoleDown () {
		if(prevIndex > 0) {
			prevIndex--;
			consoleInput.text = prevEntries [prevIndex];
		}
		MoveCaretToEnd ();
	}

	// Move caret to end
	void MoveCaretToEnd() {
		consoleInput.text = prevEntries [prevIndex];
		consoleInput.MoveTextEnd (false);
	}

	// Return feedback on console input
	void ConsoleFeedback(string feedback, bool isGood) {
		feedbackGood = isGood;

		StopCoroutine ("HideFeedback");

		// Change the colour to represent the feedback given
		feedbackText.color = new Color (1, 1, 1);
		if (!isGood) {
			feedbackText.color = new Color (0.7f, 0.7f, 0.7f);
		}

		feedbackText.text = feedback;
		ShowFeedbackContainer ();

		StartCoroutine ("HideFeedback");
	}

	IEnumerator HideFeedback() {
		if (feedbackGood) {
			yield return new WaitForSeconds (destroyGoodAfter);
		} else {
			yield return new WaitForSeconds (destroyBadAfter);
		}
		feedbackText.text = "";
		CloseFeedbackContainer ();
	}

	// Call me to open the console
	void OpenConsole() {
		// Open the console
		prevIndex = 0;
		consoleContainer.SetActive (true);
		consoleInput.ActivateInputField (); // src: https://forum.unity.com/threads/unity-4-6-ui-how-to-prevent-deselect-losing-focus-in-inputfield.272387#post-1922941 (24 Jan 2018)
		consoleOpen = true;
	}

	void CloseConsole() {
		// Reset the console, ready to be used again
		consoleOpen = false;
		consoleInput.text = "";
		consoleContainer.SetActive(false);
		CloseFeedbackContainer ();
	}

	void ShowFeedbackContainer() {
		feedbackContainer.SetActive (true);
	}

	void CloseFeedbackContainer() {
		feedbackContainer.SetActive (false);
	}

	string FormatInput(string input) {
		// set to lowercase
		input = input.ToLower ();

		// remove forward slash
		if (input [0] == '/') {
			input = input.TrimStart ('/');
		}

		return input;
	}

	void ProcessConsoleInput(string input) {
		if (input != "" && consoleOpen) {
			input = FormatInput (input);
			string[] parameters = input.Split (' ');

			// Call the appropriate function
			AllCommands(parameters[0], parameters);

			// Reset input afterwards
			consoleInput.text = "";
			consoleInput.ActivateInputField ();

			// Let's also add it to the array
			prevEntries.Insert(1, input);
		}
	}

	bool isInt(string input) {
		int result;
		return int.TryParse (input, out result);
	}

	bool isFloat(string input) {
		float result;
		return float.TryParse (input, out result);
	}


	/*   CONSOLE COMMANDS GO HERE */

	void AllCommands(string command, string[] parameters) {
		
		// Kill the player
		if (command == "kill" || command == "die") {
			playerEntity.ReduceHealth (999999);
			ConsoleFeedback ("Player killed", true);
		}

		// Heal the player
		else if (command == "heal") {
			// Heal to full health
			if (parameters.Length == 1) {
				playerEntity.IncreaseHealth (99999);
				ConsoleFeedback ("Player healed to full health", true);
			}

			// Heal to defined amount
			else {
				// Make sure that the parameter is a number
				if (isInt (parameters [1])) {
					playerEntity.IncreaseHealth (int.Parse (parameters [1]));
					ConsoleFeedback ("Player healed by " + parameters [1], true);
				} else {
					ConsoleFeedback ("Unable to heal by non-numerical value", false);
				}
			}
		}

		// Change the time scale
		else if (command == "timescale") {
			if (parameters.Length == 1) {
				ConsoleFeedback ("Time scale must take a parameter", false);
			} else {
				if (isInt (parameters [1])) {
					Time.timeScale = int.Parse (parameters [1]);
					ConsoleFeedback ("Timescale set to " + parameters [1], true);
				} else {
					ConsoleFeedback ("Cannot change time scale by non-numerical value", false);
				}
			}
		}

		// Player invincibility
		else if (command == "invincible") {
			if (parameters.Length == 1) {
				ConsoleFeedback ("Command requires a second parameter (on/off)", false);
			} else {
				if (parameters [1] == "on") {
					playerEntity.invincible = true;
					ConsoleFeedback ("Player is now invincible", true);
				} else if (parameters [1] == "off") {
					playerEntity.invincible = false;
					ConsoleFeedback ("Player is no longer invincible", true);
				} else {
					ConsoleFeedback ("Command requires second parameter to be either (on/off)", false);
				}
			}
		}

		// Wipe the inventory
		else if (command == "clearinventory") {
			// Wipe the player's inventory
			player.GetComponent<Inventory> ().ClearInventory ();
			ConsoleFeedback ("Player inventory has been cleared", true);
		}

		// Reset story progress
		else if (command == "resetstory") {
			if (parameters.Length == 1) {
				checkpointManager.ResetProgress ();
				ConsoleFeedback ("Story has been reset", true);
			}
		}

		// Noclip
		else if (command == "noclip") {
			if (parameters.Length == 2) {
				if (parameters [1] == "on") {
					player.GetComponent<Player> ().Noclip = true;
					player.GetComponent<NavMeshAgent> ().enabled = false;
					player.transform.Find ("Body_Parts").GetComponent<BoxCollider> ().enabled = false;
					ConsoleFeedback ("Noclip enabled", true);
				} else {
					player.GetComponent<Player> ().Noclip = false;
					player.GetComponent<NavMeshAgent> ().enabled = true;
					player.transform.Find ("Body_Parts").GetComponent<BoxCollider> ().enabled = true;
					ConsoleFeedback ("Noclip disabled", true);
				}
			} else {
				ConsoleFeedback ("Command takes two parameters: /noclip on/off", false);
			}
		}

		// Killall
		else if (command == "killall") {
			storyManager.KillAllEnemies (true, true);
			ConsoleFeedback ("Killed all enemies", true);
		}

		// Loadscene
		else if (command == "loadscene") {
			if (parameters.Length == 2) {
				if (isInt (parameters [1])) {
					SceneManager.LoadScene (int.Parse(parameters [1]));
				}
			} else {
				ConsoleFeedback ("Command takes two parameters: /loadscene <index>", false);
			}
		}

		// Give a pickup
		else if (command == "give") {
			if (parameters.Length == 2) {
				GameObject obj;
				if (isInt(parameters [1])) {
					// passed in the ID
					obj = GameObject.Find ("Database").transform.Find ("Pickups").GetComponent<DatabasePickups> ().GetPickupByID (int.Parse(parameters [1]));
				} else {
					// passed in the name
					obj = GameObject.Find ("Database").transform.Find ("Pickups").GetComponent<DatabasePickups> ().GetPickup (parameters[1]);
				}

				if (obj != null) {
					GameObject newObj = Instantiate (obj);
					newObj.name = obj.name;
					player.GetComponent<Inventory> ().AddItemToInventory (newObj);
					ConsoleFeedback ("Player successfully given item", true);
					CloseConsole ();
				} else {
					ConsoleFeedback ("Item " + parameters [1] + " not found", false);
				}
			} else {
				ConsoleFeedback ("Command takes two parameters: /give <item name/item id>", false);
			}
		}

		// Command is not found
		else {
			ConsoleFeedback ("Command not found", false);
		}
	}



}
