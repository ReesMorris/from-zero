using UnityEngine;

// This script is necessary as making the player an AudioListener
// would mean that sound direction is relative to player rotation,
// rather than camera rotation. This makes a very unwanted effect.
public class AudioListener : MonoBehaviour {

	public GameObject player;
	public GameObject mainCamera;

	void Start() {
		transform.eulerAngles = new Vector3(0f, mainCamera.transform.eulerAngles.y, 0f);
	}

	void Update () {
		transform.position = player.transform.position;
	}
}
