using UnityEngine;

public class Story_ZombieCar : MonoBehaviour {

	private bool collided;
	public bool Collided {
		get {
			return collided;
		}
	}

	void Start() {
		collided = false;
	}

	void OnTriggerEnter(Collider collider) {
		if (collider.name == "Zombie") {
			collided = true;
		}
	}
}
