using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

	private List<CustomInput> inputs;
	public List<CustomInput> Inputs {
		get {
			return inputs;
		}
	}

	private string lastKeyUp;

	// Use this for initialization
	void Start () {
		SetDefaultInputs ();
	}

	// (Re)set the default input values
	public void SetDefaultInputs() {
		inputs = new List<CustomInput> ();

		/*     Player Movement     */
		AddInput ("MOVE_FORWARD", KeyCode.W, true, true);
		AddInput ("MOVE_LEFT", KeyCode.A, true, true);
		AddInput ("MOVE_BACKWARD", KeyCode.S, true, true);
		AddInput ("MOVE_RIGHT", KeyCode.D, true, true);
		AddInput ("MOVE_SPRINT", KeyCode.LeftShift, true, true);

		/*     Weapon Controls     */
		AddInput ("WEAPON_FIRE", KeyCode.Mouse0, true, true);
		AddInput ("WEAPON_RELOAD", KeyCode.R, true, true);

		/*     Camera Controls     */
		AddInput ("CAMERA_ZOOM_IN", KeyCode.Q, true, true);
		AddInput ("CAMERA_ZOOM_OUT", KeyCode.E, true, true);

		/*     Inventory Controls     */
		AddInput ("INVENTORY_DROP", KeyCode.G, true, true);
		AddInput ("INVENTORY_PICK", KeyCode.F, true, true);
		AddInput ("INVENTORY_CYCLE", KeyCode.Tab, true, true);
		AddInput ("INVENTORY_SLOT_1", KeyCode.Alpha1, true, true);
		AddInput ("INVENTORY_SLOT_2", KeyCode.Alpha2, true, true);
		AddInput ("INVENTORY_SLOT_3", KeyCode.Alpha3, true, true);
		AddInput ("INVENTORY_SLOT_4", KeyCode.Alpha4, true, false);
		AddInput ("INVENTORY_SLOT_5", KeyCode.Alpha5, true, false);
		AddInput ("INVENTORY_SLOT_6", KeyCode.Alpha6, true, false);
		AddInput ("INVENTORY_SLOT_7", KeyCode.Alpha7, true, false);

		/*     Console Keys     */
		AddInput ("CONSOLE_OPEN", KeyCode.Slash, true, true);
		AddInput ("CONSOLE_CLOSE", KeyCode.Escape, false, true);
		AddInput ("CONSOLE_SUBMIT", KeyCode.Return, false, true);
		AddInput ("CONSOLE_CYCLE_UP", KeyCode.UpArrow, false, true);
		AddInput ("CONSOLE_CYCLE_DOWN", KeyCode.DownArrow, false, true);

		/*     UI Controls     */
		AddInput ("UI_PAUSE_GAME", KeyCode.Escape, false, false);
		AddInput ("UI_READABLE_HIDE", KeyCode.Escape, false, true);
	}

	// Add a new input to the array
	void AddInput(string name, KeyCode keyCode, bool allowRebinding, bool showInControls) {
		if (PrefExists (name)) {
			keyCode = GetKeyFromPref (name);
		}
		inputs.Add(new CustomInput(name, keyCode, allowRebinding, showInControls));
		UpdatePrefs (name, keyCode);
	}

	// Returns true if the user's input matches the key we're looking for
	public bool InputMatches(string key) {
		if (Input.anyKey && inputs != null) {
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs [i].name == key) {
					if (Input.GetKey (inputs [i].keyCode)) {
						return true;
					}
					return false;
				}
			}
		}
		return false;
	}

	// Returns true if the user's key up matches the key we're looking for
	public bool InputMatchesKeyup(string key) {
		if (inputs != null) {
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs [i].name == key) {
					if (Input.GetKeyUp (inputs [i].keyCode)) {
						return true;
					}
				}
			}
		}
		return false;
	}

	// Return the key as a string
	public string GetKeyValue(string key) {
		if(inputs == null) {
			SetDefaultInputs ();
		}
		for (int i = 0; i < inputs.Count; i++) {
			if (inputs [i].name == key) {
				return inputs [i].keyCode.ToString();
			}
		}

		// Key is missing (shouldn't happen, but just to be sure)
		return "{" + key + "}";
	}

	public void UpdateInputKey(string name, KeyCode newKey) {
		for (int i = 0; i < inputs.Count; i++) {
			if (inputs [i].name == name) {
				inputs [i].keyCode = newKey;
				UpdatePrefs (name, newKey);
				break;
			}
		}
	}

	void UpdatePrefs(string name, KeyCode keyCode) {
		PlayerPrefs.SetInt(name, (int) keyCode);
	}

	KeyCode GetKeyFromPref(string name) {
		return (KeyCode)PlayerPrefs.GetInt(name);
	}

	bool PrefExists(string name) {
		return PlayerPrefs.HasKey (name);
	}
}
