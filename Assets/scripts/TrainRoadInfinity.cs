using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainRoadInfinity : MonoBehaviour {

	public GameObject offroad;
	public float resetAt;

	[Range(0, 2)]
	public float speed;

	private Vector3 startPos;
	private bool started;

	void Start () {
		started = false;
		startPos = offroad.transform.position;
	}

	public void Move() {
		started = true;
	}

	void Update () {
		if (started) {
			Vector3 actSpeed = Vector3.zero;
			if (Time.timeScale != 0)
				actSpeed = (Vector3.back * -speed);
			offroad.transform.position += actSpeed;

			if (offroad.transform.position.z > resetAt) {
				offroad.transform.position = startPos;
			}
		}
	}

	public void SpeedUp(int maxSpeed) {
		StartCoroutine (Speed (maxSpeed));
	}

	IEnumerator Speed(int maxSpeed) {
		while (speed < maxSpeed) {
			speed += 0.01f;
			yield return new WaitForSeconds (0.1f);
		}
	}
}
