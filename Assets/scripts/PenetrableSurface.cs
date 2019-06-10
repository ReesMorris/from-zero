using UnityEngine;

public class PenetrableSurface : MonoBehaviour {

	public int penetrationReduction;
	[Range(0, 100)]
	public float damageReduction;

	void Start () {
		gameObject.tag = "PenetrableSurface";
		gameObject.layer = 14;
	}
}
