using UnityEngine;

public class InvisibleWall : MonoBehaviour {

	void Start () {
		GetComponent<MeshRenderer> ().enabled = false;
	}
}
