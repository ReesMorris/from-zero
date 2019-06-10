using UnityEngine;

[System.Serializable]
public class TrainModule {

	public GameObject module;
	[Range(0, 100)]
	public int chanceOfAppear;
}
