using UnityEngine;
using UnityEngine.Animations;

[System.Serializable] // [5]
public class Story {
	public GameObject[] entities;
	public GameObject[] destinations;
	public GameObject[] spawners;
	public AudioSource[] audio;
	public Animator[] animators;
}
