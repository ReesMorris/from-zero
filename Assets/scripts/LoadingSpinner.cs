using UnityEngine;

public class LoadingSpinner : MonoBehaviour {

	public float speed;

	void Update () {
		transform.Rotate(Vector3.back * Time.deltaTime * speed);
	}
}
