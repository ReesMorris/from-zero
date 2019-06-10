using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorwayInfinity : MonoBehaviour {

	public GameObject[] motorways;
	public float resetAt;

	[Range(0, 2)]
	public float speed;

	private int currentTarget;
	private List<Vector3> initialPoints;

	void Start() {
		Init ();
		currentTarget = 0;

		// Just to be sure
		if (resetAt < 0) {
			resetAt *= -1;
		}
	}

	public void Slow() {
		StartCoroutine(SlowDown());
	}

	void Init() {
		initialPoints = new List<Vector3> ();
		for (int i = 0; i < motorways.Length; i++) {
			initialPoints.Add (motorways [i].transform.position);
		}
	}

	void Update () {
		for (int i = 0; i < motorways.Length; i++) {
			Vector3 actSpeed = Vector3.zero;
			if (Time.timeScale != 0)
				actSpeed = (Vector3.left * speed);
			
			motorways [i].transform.position += actSpeed;
		}

		if (motorways [currentTarget].transform.position.x <= -resetAt) {
			motorways [currentTarget].transform.position = Vector3.right * (resetAt * (motorways.Length - 1));
			currentTarget = (currentTarget + 1) % motorways.Length;
		}
	}

	public void Reset() {
		speed = 0f;
		for (int i = 0; i < motorways.Length; i++) {
			motorways [i].transform.position = initialPoints [i];
		}
	}

	IEnumerator SlowDown() {
		while (speed > 0) {
			speed -= 0.01f;
			yield return new WaitForSeconds (0.1f);
		}
		speed = 0f;
	}
}
