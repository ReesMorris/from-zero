using UnityEngine;

public class CustomInput {

	public string name;
	public KeyCode keyCode;
	public bool allowRebinding;
	public bool displayInControls;

	public CustomInput(string _name, KeyCode _keyCode, bool _allowRebinding, bool _displayInControls) {
		name = _name;
		keyCode = _keyCode;
		allowRebinding = _allowRebinding;
		displayInControls = _displayInControls;
	}
}
