using UnityEngine;
using UnityEngine.Animations;

public class Animations : MonoBehaviour {

	private Animator animator;

	public void PlayAnimationClip(string clipName) {
		if (animator == null) {
			animator = GetComponent<Animator> ();
		}

		animator.Play (clipName);
	}
}
