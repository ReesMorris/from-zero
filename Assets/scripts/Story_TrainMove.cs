using UnityEngine;
using System.Collections;

public class Story_TrainMove : MonoBehaviour {

	public GameObject noRoof;
	private bool going;
	private float speed;

	void Update () {
		if (going) {
			Vector3 mspeed = (Vector3.right * Time.deltaTime * speed);
			transform.position += mspeed;
		}
	}

	public void Move() {
		going = true;
		speed = 0.05f;
		StartCoroutine ("IncreaseSpeed");
		noRoof.SetActive (false);
	}

	IEnumerator IncreaseSpeed() {
		while (speed < 40f) {
			speed += (speed * 0.1f);
			yield return new WaitForSeconds (0.01f);
		}
	}
}
