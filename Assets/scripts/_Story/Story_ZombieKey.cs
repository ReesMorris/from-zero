using UnityEngine;

public class Story_ZombieKey : MonoBehaviour {

	public AudioSource isEverythingAlright;
	public GameObject key;

	private Entity entity;
	private SoundManager soundManager;
	private bool playedSound;

	void Start () {
		entity = GetComponent<Entity> ();
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
	}

	void Update () {
		if (entity.CurrentHealth < entity.maxHealth && !playedSound) {
			playedSound = true;
			soundManager.PlaySound (isEverythingAlright, 0.5f);
		}
		if (entity.IsDead && PlayerPrefs.GetInt("key_state") == 0) {
			PlayerPrefs.SetInt ("key_state", 1);
			Instantiate (key, transform.position, Quaternion.identity);
		}
	}
}
