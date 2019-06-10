using UnityEngine;
using UnityEngine.Animations;

public class CutsceneCamera : MonoBehaviour {

	private Animator animator;

	// Public function which will start a cutscene clip running
	public void StartCutscene(string clipName) {
		// The camera is not active at the start, so the game will not find it
		// Putting the code here will ensure that we have it
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		animator.Play (clipName);
	}
}
