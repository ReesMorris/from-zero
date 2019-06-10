using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using System.Reflection;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour {

	public string firstStoryName;
	public GameObject storySkip;
	public GameObject player;
	public Story story;

	// Other class requirements
	private GameManager gameManager;
	private CutsceneManager cutsceneManager;
	private TutorialManager tutorialManager;
	private SoundManager soundManager;
	private Pathfinding playerPathfinding;
	private NavMeshAgent playerNavMeshAgent;
	private PlayerStats playerStats;
	private Gun playerGun;
	private ChoiceManager choiceManager;
	private CheckpointManager checkpointManager;
	private UIManager uiManager;
	private Entity playerEntity;
	private Inventory playerInventory;
	private bool skipping = false;
	private int choice = -1;
	private bool spawnersActivated = false;
	public bool Skipping {
		get {
			return skipping;
		}
	}

	void Start() {
		GameObject gm = GameObject.Find ("GameManager");
		gameManager = gm.GetComponent<GameManager> ();
		cutsceneManager = gm.GetComponent<CutsceneManager> ();
		tutorialManager = gm.GetComponent<TutorialManager> ();
		soundManager = gm.GetComponent<SoundManager> ();
		playerStats = gm.GetComponent<PlayerStats> ();
		checkpointManager = gm.GetComponent<CheckpointManager> ();
		choiceManager = gm.GetComponent<ChoiceManager> ();
		uiManager = gm.GetComponent<UIManager> ();
		playerInventory = player.GetComponent<Inventory> ();

		playerPathfinding = player.GetComponent<Pathfinding> ();
		playerNavMeshAgent = player.GetComponent<NavMeshAgent> ();
		playerEntity = player.GetComponent<Entity> ();
	}

	// Call this to skip the current cutscene
	[System.Obsolete("Cutscenes can no longer be skipped.")]
	public void SkipCutscene(bool forceSkip) {
		// Forceskip will force a cutscene to skip even if it is disabled in the GameManager.cs script
		if ((cutsceneManager.CutsceneActive && gameManager.canSkipCutscenes) || forceSkip) {
			// Only allow the user to skip if a choice is not pending
			if (!choiceManager.ChoicePending) {
				// Show the loading screen and increase the timescale to maximum
				skipping = true;
				storySkip.SetActive (true);
				Time.timeScale = 100f;
				for (int i = 0; i < story.audio.Length; i++) {
					soundManager.StopSound (story.audio [i]);
				}
			}
		}
		for (int i = 0; i < story.audio.Length; i++) {
			soundManager.StopSound (story.audio [i]);
		}
	}

	// Called before a choice is made, or at the end of a story
	[System.Obsolete("Cutscenes can no longer be skipped.")]
	public void StopCutsceneSkip() {
		skipping = false;
		storySkip.SetActive (false);
		Time.timeScale = 1f;
		PlayerPrefs.SetInt ("QuickReset", 0);
	}

	// Call this if you want to teleport an entity, as it will disable their pathfinding first (forcing them to be in that position)
	public void TeleportEntity(GameObject entity, Vector3 position) {
		NavMeshAgent navMeshAgent;

		// Get the navMeshAgent of the entity
		if (entity.name == "Player") {
			navMeshAgent = playerNavMeshAgent;
		} else {
			navMeshAgent = entity.GetComponent<NavMeshAgent> ();
		}

		// Temporarily disable the entity's navMeshAgent to ignore clipping, teleport them, then enable it again
		navMeshAgent.enabled = false;
		entity.transform.position = position;
		navMeshAgent.enabled = true;
	}

	// Call this function to start a story, just pass in the name of the coroutine and whether we skip all of the cutscenes
	// Quickrun will force a skip of the story, placing the player at the last point of it
	[System.Obsolete("QuickRun feature no longer exists. Please call StartStory(string) instead.")]
	public void StartStory(string name, bool _quickrun) {
		StartCoroutine (name);
	}

	// Call this function to start a story, just pass in the name of the coroutine
	public void StartStory(string name) {
		StartCoroutine (name);
	}

	// Called when a story is first started, setting default values and properties
	void OnStoryStart(string storyName) {
		Time.timeScale = 1f;

		// Update the checkpoint manager
		if (checkpointManager == null) {
			Start ();
		}
		checkpointManager.SetCheckpoint (storyName);

		// Update player info
		playerPathfinding.target = null;
		playerPathfinding.enabled = true;
		playerNavMeshAgent.enabled = true;
	}

	void OnStoryEnd() {
		// End the cutscene
		cutsceneManager.EndCutscene ();

		// Update player details
		playerPathfinding.target = null;
		//playerNavMeshAgent.enabled = false;
		playerPathfinding.enabled = false;
		player.GetComponent<Entity> ().StopLookAt ();

		// Reset the quickrun details
		//StopCutsceneSkip();
	}

	// Call this function to play a cutscene
	void PlayCutscene(string clipName) {
		cutsceneManager.PlayCutscene (clipName);
	}

	// Load a scene
	void LoadScene(string name) {
		//StopCutsceneSkip ();
		SceneManager.LoadScene(name);
	}

	// Play a sound
	void PlaySound(int index, float delay) {
		soundManager.PlaySound (story.audio [index], delay);
	}

	// Stop a sound
	void StopSound(int index) {
		soundManager.StopSound (story.audio [index]);
	}

	// Get an entity
	GameObject GetEntity(int index) {
		return story.entities[index];
	}

	// Play an animation
	void PlayAnimation(int index, string name) {
		story.animators [index].Play (name);
	}

	// Fade sound
	void FadeSound(int index, float time) {
		soundManager.FadeSound (story.audio [index], time);
	}

	// Is a sound playing?
	bool SoundIsPlaying(int index) {
		return story.audio [index].isPlaying;
	}

	// Return a story entity
	GameObject GetGameObject(int index) {
		return story.entities[index];
	}

	// Show a tutorial
	void QueueTutorial(string key, float duration) {
		tutorialManager.QueueTutorial (key, duration);
	}

	// Let an entity shoot
	void EnableEntityShooting(int index) {
		GameObject entity = GetEntity (index);
		entity.GetComponent<Animator> ().enabled = false;
		entity.GetComponent<Inventory> ().enabled = true;
		entity.GetComponent<Survivor> ().enabled = true;
		entity.GetComponent<Pathfinding> ().enabled = true;
	}

	// Prevent an entity from shooting
	void DisableEntityShooting(int index) {
		GameObject entity = GetEntity (index);
		entity.GetComponent<Animator> ().enabled = true;
		entity.GetComponent<Inventory> ().enabled = false;
		entity.GetComponent<Survivor> ().enabled = false;
		entity.GetComponent<Pathfinding> ().enabled = false;
	}

	// Kill all enemies
	public void KillAllEnemies(bool destroy, bool incPlayerStats) {
		// Zombies
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Zombie");
		if (incPlayerStats)
			playerStats.zombieKills += enemies.Length;
		foreach (GameObject enemy in enemies) {
			if (destroy)
				Destroy (enemy);
			else
				enemy.GetComponent<Entity> ().ReduceHealth (99999);
		}

		// Survivors
		enemies = GameObject.FindGameObjectsWithTag("Survivor");
		foreach (GameObject enemy in enemies) {
			Survivor survivor = enemy.GetComponent<Survivor> ();
			if (survivor) {
				if (survivor.type == Survivor.Type.Hostile) {
					if (destroy)
						Destroy (enemy);
					else
						enemy.GetComponent<Entity> ().ReduceHealth (99999);
				}
			}
		}
	}

	// Activate all spawners
	void ActivateSpawners() {
		if (!spawnersActivated) {
			for (int i = 0; i < story.spawners.Length; i++) {
				story.spawners [i].GetComponent<Spawner> ().Spawn ();
			}
		}
	}


	/*      Debug      */

	IEnumerator Debug_Freeroam() {
		yield return new WaitForSeconds (0f);
		OnStoryStart ("Debug_Freeroam");
		OnStoryEnd ();
	}

	/*      Stories      */

	IEnumerator Story_Airport_0() {
		OnStoryStart ("Story_Airport_0");

		// First story of this episode
		PlayerPrefs.SetInt("currentEpisodeNumber", 0);

		uiManager.ShowBlackScreen ();
		PlayCutscene ("nothing"); // prevents us from being able to walk around, shoot, etc
		PlaySound (0, 0f); // airplane sfx
		yield return new WaitForSeconds (2f);
		PlaySound (1, 0f); // ding dong sfx
		yield return new WaitForSeconds (2f);
		PlaySound (2, 0f); // announcement
		yield return new WaitForSeconds (9.5f);
		PlaySound (3, 0f); // music
		yield return new WaitForSeconds (3f);
		StopSound (0); // stop airplane sfx
		yield return new WaitForSeconds (5.2f);
		uiManager.ShowLogo ();
		yield return new WaitForSeconds (3f);
		uiManager.FadeOut (10f);
		PlayCutscene ("airport_1");
		yield return new WaitForSeconds (2f);
		uiManager.FadeLogo (5f);
		yield return new WaitForSeconds (4f);
		PlayAnimation (1, "world_anims");
		yield return new WaitForSeconds (5.7f);
		PlayAnimation (0, "walking_suitcase");
		yield return new WaitForSeconds(1.5f);
		PlayCutscene ("airport_2");
		yield return new WaitForSeconds (2.3f);
		PlaySound (4, 5f); // cabbie dialogue 1
		FadeSound (3, 7.5f); // music
		PlayAnimation (0, "suitcase_boot");
		yield return new WaitForSeconds (2.3f);
		PlayAnimation (2, "taxi_driver_1");
		yield return new WaitForSeconds (7f);
		PlayCutscene ("airport_3");
		PlayAnimation (0, "enter_taxi");
		yield return new WaitForSeconds (0.8f);
		PlayAnimation (2, "taxi_driver_2");
		PlaySound (5, 3.5f); // cabbie dialogue 2
		yield return new WaitForSeconds (4f);
		PlayAnimation (1, "world_anims_2");
		yield return new WaitForSeconds (8f);
		uiManager.ShowBlackLoadingScreen ();
		yield return new WaitForSeconds (1f);

		LoadScene ("(G2) Highway");
	}

	IEnumerator Story_Highway_0() {
		OnStoryStart ("Story_Highway_0");
		MotorwayInfinity mf = GameObject.Find ("GameManager").GetComponent<MotorwayInfinity> ();

		PlayCutscene ("highway_1");
		PlayAnimation (1, "sit_in_taxi");
		PlayAnimation (2, "sit_in_taxi_driver");
		PlaySound (4, 0f); // inside cab

		yield return new WaitForSeconds (1f);
		PlaySound (0, 0f); // cabbie question
		yield return new WaitForSeconds (3.3f);


		// choice start
		choiceManager.SetupChoiceSequence(5, -1, "choice_i_live_here", "choice_its_not_important");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		PlayAnimation (0, "world_anims_highway_1");

		if (choice == 0) {
			PlaySound (1, 0f); // good response
			yield return new WaitForSeconds (6f);
		} else if (choice == 1) {
			PlaySound (2, 0f); // bad response
			yield return new WaitForSeconds (8f);
		} else {
			yield return new WaitForSeconds (0.4f);
		}
		// choice end

		PlaySound (3, 0f); // cabbie talks

		yield return new WaitForSeconds (3f); // make sure there is a 'road' behind the player at this number!
		PlayAnimation (0, "world_anims_police");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("highway_2");
		yield return new WaitForSeconds (5.5f);
		PlayCutscene ("highway_1");
		yield return new WaitForSeconds (10f);
		PlayCutscene ("highway_4");

		// wait for the dialogue to end
		while (SoundIsPlaying (3)) {
			yield return new WaitForSeconds (1f);
		}

		PlayCutscene ("highway_3");

		// choice start
		choiceManager.SetupChoiceSequence(5, -1, "choice_you_have_a_mark", "choice_its_just_a_hoax", "choice_empty");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (5, 0f); // you have a scratch
			yield return new WaitForSeconds (6f);
		} else if (choice == 1) {
			PlaySound (6, 0f); // its just a hoax
			yield return new WaitForSeconds (5.8f);
		} else {
			yield return new WaitForSeconds (0.4f);
		}
		// choice end

		PlaySound (7, 0f); // cabbie tells story

		yield return new WaitForSeconds (8f);
		PlayCutscene ("highway_8");
		yield return new WaitForSeconds (22f);
		PlayCutscene ("highway_3");

		while (SoundIsPlaying (7)) {
			yield return new WaitForSeconds (1f);
		}
		yield return new WaitForSeconds (0.5f);

		PlaySound (8, 0f); // did it hurt?

		yield return new WaitForSeconds (1f);
		while (SoundIsPlaying (8)) {
			yield return new WaitForSeconds (1f);
		}

		mf.Slow (); // stop the environment moving
		PlaySound (9, 1.8f); // traffic complaint

		// set motorway position
		yield return new WaitForSeconds (3f);
		mf.Reset (); // reset back to original state, so we can move it based on there
		//GetGameObject (0).transform.position = new Vector3 (-90f, 1.34f, -28.43f);

		PlayCutscene ("highway_5");
		yield return new WaitForSeconds (8f);
		PlayCutscene ("highway_6");
		yield return new WaitForSeconds (2.1f);
		PlayAnimation (4, "taxi_turn");

		yield return new WaitForSeconds (4f);

		StartStory ("Story_Highway_1");
	}

	IEnumerator Story_Highway_1() {
		OnStoryStart ("Story_Highway_1");

		PlayAnimation (4, "taxi_offroad");
		PlayCutscene ("highway_7");
		PlayAnimation (1, "sit_in_taxi_2");
		PlayAnimation (2, "sit_in_taxi_driver_2");
		yield return new WaitForSeconds (0.1f);
		GetComponent<OffroadInfinity> ().Move ();

		yield return new WaitForSeconds (1f);
		PlaySound (10, 0f); // driver begins turning
		yield return new WaitForSeconds(1f);

		while (SoundIsPlaying (10)) {
			yield return new WaitForSeconds (1f);
		}

		// choice start
		choiceManager.SetupChoiceSequence(5, 2, "choice_whos_clara", "choice_are_you_okay", "choice_i_dont_care");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (11, 0f); // whos clara
			yield return new WaitForSeconds (1.5f);
		} else if (choice == 1) {
			PlaySound (12, 0f); // are you okay
			yield return new WaitForSeconds (2f);
		} else {
			PlaySound (13, 0f); // i dont care
			yield return new WaitForSeconds (4f);
		}
		// choice end

		PlaySound (14, 0f); // final conversation

		yield return new WaitForSeconds (8f);

		uiManager.ShowBlackScreen ();

		yield return new WaitForSeconds (2f);

		uiManager.ShowBlackLoadingScreen ();

		LoadScene ("(G3) Zack's House");
	}



	IEnumerator Story_Zack_House_0() {
		OnStoryStart ("Story_Zack_House_0");

		PlayCutscene ("zacks_house_1");
		uiManager.ShowBlackScreen ();
		yield return new WaitForSeconds (0.5f);

		// choice start
		choiceManager.SetupChoiceSequence(5, 0, "choice_shout_for_help", "choice_ask_if_okay");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		PlayCutscene ("zacks_house_2");
		choice = choiceManager.GetChoice ();

		uiManager.FadeOut (5f);
		PlayAnimation (0, "injured_ground");
		PlayAnimation (1, "injured_taxi");

		if (choice == 0) {
			PlaySound (0, 0f); // help
			yield return new WaitForSeconds (4f);
		} else {
			PlaySound (1, 0f); // are you okay?
			yield return new WaitForSeconds (3.6f);
		}
		// choice end

		PlaySound (2, 0f); // zombie growl
		PlayAnimation (1, "injured_taxi_2");

		yield return new WaitForSeconds (2.5f);

		PlayAnimation (1, "injured_taxi_3");
		PlaySound (3, 0f); // frank says wait here

		yield return new WaitForSeconds (4f);

		PlaySound (2, 0f); // zombie growl
		PlayAnimation (1, "injured_taxi_4"); // looping arms

		yield return new WaitForSeconds (2f);

		PlaySound (4, 0f); // what do you want?

		yield return new WaitForSeconds (4f);

		// housekeeping, continuity, etc
		playerInventory.ClearInventory(); // just in case
		GetGameObject(0).SetActive(false); // hide the fake player anim
		TeleportEntity (player, GetGameObject (1).transform.position); // set the player there
		player.transform.LookAt(GetGameObject(3).transform.position);
		GetGameObject (10).SetActive (true); // trigger story
		yield return new WaitForSeconds (0.2f);
		PlayCutscene ("zacks_house_3");

		yield return new WaitForSeconds (1f);

		StartStory ("Story_Zack_House_1");

	}

	IEnumerator Story_Zack_House_1() {
		OnStoryStart ("Story_Zack_House_1");

		// housekeeping, continuity, etc
		PlayCutscene ("zacks_house_3");
		playerInventory.ClearInventory(); // just in case
		GetGameObject(0).SetActive(false); // hide the fake player anim
		TeleportEntity (player, GetGameObject (1).transform.position); // set the player instead of fake player
		player.transform.LookAt(GetGameObject(3).transform.position);
		GetGameObject (10).SetActive (true); // trigger story
		PlayAnimation (1, "injured_taxi_4"); // looping arms

		OnStoryEnd ();

		// tutorial time!
		yield return new WaitForSeconds(1f);
		QueueTutorial("tutorial_text_intro", 2f);
		QueueTutorial("tutorial_text_move_forward", 3f);
	}

	IEnumerator Story_Zack_House_2() {
		OnStoryStart ("Story_Zack_House_2");

		TeleportEntity (player, GetGameObject (4).transform.position); // move the player off camera
		GetGameObject(0).SetActive(true); // add the fake player on camera

		PlayAnimation (0, "enter_zack_house");

		PlayCutscene ("zacks_house_4");
		PlayAnimation (2, "zack_holding_weapon");

		yield return new WaitForSeconds (1.5f);

		PlayAnimation (3, "sliding_door_move");

		yield return new WaitForSeconds (1.5f);
		PlaySound (5, 0f);
		yield return new WaitForSeconds (2f);
		PlayCutscene ("zacks_house_5");
		yield return new WaitForSeconds (2f);

		// choice start
		choiceManager.SetupChoiceSequence(2f, 1, "choice_i_need_help", "choice_i_was_scared", "choice_i_something_is_wrong");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayerPrefs.SetInt ("frank_help_choice", choice);

		if (choice == 0) {
			PlaySound (6, 0f); // i need help
			yield return new WaitForSeconds (1.3f);
		} else if(choice == 1) {
			PlaySound (7, 0f); // i was scared
			yield return new WaitForSeconds (1.3f);
		} else {
			PlaySound (8, 0f); // something is wrong
			yield return new WaitForSeconds (1.5f);
		}
		// choice end

		PlaySound (9, 0f);
		PlayCutscene ("zacks_house_6");

		yield return new WaitForSeconds (5f);
		PlayAnimation (2, "zack_weapon_down");
		PlayCutscene ("zacks_house_8");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("zacks_house_6");
		while (SoundIsPlaying (9)) {
			yield return new WaitForSeconds (1f);
		}

		PlayCutscene ("zacks_house_7");

		// choice start
		choiceManager.SetupChoiceSequence(5f, 1, "choice_what_is_going_on", "choice_why_are_you_helping_me", "choice_why_should_i_trust_you");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (10, 0f); // what is going on
			yield return new WaitForSeconds (8f);
		} else if(choice == 1) {
			PlaySound (11, 0f); // why are you helping me
			yield return new WaitForSeconds (13f);
		} else {
			PlaySound (12, 0f); // why should I trust you
			yield return new WaitForSeconds (6f);
		}
		// choice end
		PlayCutscene ("zacks_house_9");

		PlaySound (13, 0f); // zack goes upstairs
		PlayAnimation (2, "zack_go_upstairs");

		yield return new WaitForSeconds (5f);

		StartStory ("Story_Zack_House_3");
	}

	IEnumerator Story_Zack_House_3() {
		OnStoryStart ("Story_Zack_House_3");

		// make player take over zack's position
		TeleportEntity (player, GetEntity (6).transform.position);
		GetEntity (0).SetActive (false);
		player.transform.LookAt (GetEntity(5).transform.position);
		GetEntity (7).SetActive (false);
		GetEntity (9).SetActive (false);
		GetEntity (5).SetActive (false);
		GetEntity (10).SetActive (false);
		PlayAnimation (3, "sliding_door_move");

		OnStoryEnd ();

		yield return new WaitForSeconds (5f);
		PlaySound (14, 0f); // phone rings

		// this is the exploration part. you have 5+40 (45) seconds, go nuts
		yield return new WaitForSeconds(20f);

		StartStory ("Story_Zack_House_4");
	}

	IEnumerator Story_Zack_House_4() {
		OnStoryStart ("Story_Zack_House_4");
	
		// continuity
		GetEntity(16).transform.position = Vector3.zero;
		TeleportEntity (player, GetEntity (1).transform.position);
		player.transform.LookAt (GetEntity(5).transform.position);
		GetEntity (9).SetActive (false);
		GetEntity (10).SetActive (false);
		GetEntity (0).SetActive (true);
		GetEntity (5).SetActive (true);
		GetEntity (7).SetActive (true);
		GetEntity (9).SetActive (false);

		PlayAnimation (2, "zack_go_downstairs");
		PlayCutscene ("zacks_house_10");
		PlayAnimation (0, "walk_towards_zack_stairs");
		PlayAnimation (3, "sliding_door_move");

		yield return new WaitForSeconds(1f);
		PlaySound (15, 0f); // zack goes downstairs

		yield return new WaitForSeconds(11f);


		PlayCutscene ("zacks_house_11");
		PlayAnimation (4, "desmond_knock");
		yield return new WaitForSeconds(4f);
		PlayCutscene ("zacks_house_12");
		PlayAnimation (0, "frank_look_at_desmond");
		PlayAnimation (2, "zack_look_at_desmond");
		yield return new WaitForSeconds(3f);

		// choice start
		choiceManager.SetupChoiceSequence(5f, 1, "choice_desmond_lie", "choice_desmond_truth");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayCutscene ("zacks_house_13");

		if (choice == 0) {
			PlaySound (16, 0f); // lie
			PlayerPrefs.SetInt("desmondLie", 0);
			yield return new WaitForSeconds (6.5f);
		} else {
			PlaySound (17, 0f); // truth
			PlayerPrefs.SetInt("desmondLie", 1);
			yield return new WaitForSeconds (2.7f);
		}
		// choice end

		PlaySound (18, 0f); // that's the guy!
		PlayCutscene ("zacks_house_14");
		PlayAnimation (1, "taxi_driver_attack_desmond"); // taxi driver move towards desmond
		yield return new WaitForSeconds(8f);

		PlayAnimation (4, "desmond_turn");
		yield return new WaitForSeconds(0.9f);
		PlaySound (19, 0f); // desmond screams
		PlaySound (20, 1f); // get off him!
		yield return new WaitForSeconds(1f);
		PlayAnimation (4, "desmond_fall");
		yield return new WaitForSeconds(1f);
		PlayAnimation (1, "taxi_driver_attack_desmond_2"); // taxi driver fall towards desmond
		yield return new WaitForSeconds(0.5f);
		PlayCutscene ("zacks_house_15");
		yield return new WaitForSeconds(0.5f);
		PlayAnimation (2, "zack_approach_desmond");
		yield return new WaitForSeconds (1f);
		PlayAnimation (3, "sliding_door_open");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("zacks_house_16");
		yield return new WaitForSeconds (3.5f);
		PlayAnimation (1, "taxi_driver_attack_zack");
		PlayAnimation (2, "zack_retreat_taxi_driver");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("zacks_house_17");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("zacks_house_18");
		PlayAnimation (0, "frank_retreat_taxi_driver");
		yield return new WaitForSeconds (1f);

		// choice start
		choiceManager.SetupChoiceSequence(1.8f, 0, "choice_taxi_shoot", "choice_taxi_dont_shoot");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (21, 0f); // shoot
			yield return new WaitForSeconds (0.5f);
			PlayerPrefs.SetInt ("choice_shoot_taxi_driver", 1);
		} else {
			PlaySound (22, 0f); // don't shoot!
			yield return new WaitForSeconds (2.3f);
			PlayerPrefs.SetInt ("choice_shoot_taxi_driver", 0);
		}
		// choice end
		GetEntity(5).GetComponent<Entity>().holding.Use();
		PlayAnimation (1, "taxi_driver_die");
		yield return new WaitForSeconds (0.2f);
		PlayCutscene ("zacks_house_17");
		yield return new WaitForSeconds (1.5f);
		PlaySound (23, 0f); // breakdown
		yield return new WaitForSeconds (1f);
		PlayCutscene ("zacks_house_19");
		PlayAnimation (2, "zack_breakdown_frank_taxi");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("zacks_house_20");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("zacks_house_20");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("zacks_house_21");

		if (PlayerPrefs.GetInt ("choice_shoot_taxi_driver") == 0) {
			PlaySound (25, 0f); // i know!
			yield return new WaitForSeconds(4.5f);
		} else {
			PlaySound (24, 0f); // i know! you told me to!
			yield return new WaitForSeconds(5.4f);
		}

		PlayCutscene ("zacks_house_20");
		PlaySound (26, 0f); // let's just go ...
		yield return new WaitForSeconds(3.2f);
		PlayCutscene ("zacks_house_21");
		yield return new WaitForSeconds(1f);
		GetEntity (11).SetActive (true); // the real pistol
		PlayAnimation (2, "zack_walk_outside_house");
		yield return new WaitForSeconds(3.5f);

		StartStory ("Story_Zack_House_5");
	}

	IEnumerator Story_Zack_House_5() {
		// freeroam basically
		OnStoryStart ("Story_Zack_House_5");

		// the good ol' switcheroo (continuity)
		GetEntity (11).SetActive (true); // the real pistol should spawn now
		TeleportEntity(player, GetEntity(12).transform.position); // put the player in frank's last position
		GetEntity (0).SetActive (false);
		GetEntity (5).SetActive (false); // the animation should hide zack, but just to be sure
		GetEntity (7).SetActive (false); // and there goes the roof, again!
		GetEntity (9).SetActive (false); // the light
		GetEntity (10).SetActive (false); // the story trigger
		PlayAnimation (1, "taxi_driver_die_static"); // put the anims back in place
		PlayAnimation (4, "desmond_fall_static"); // put the anims back in place
		playerInventory.ClearInventory(); // just incase we already have the pistol, for some reason

		OnStoryEnd ();

		yield return new WaitForSeconds (1f);

		// tutorial time!
		QueueTutorial("tutorial_text_inventory_pickup", 4f);

		while (playerInventory.ActualInventoryCount () == 0) {
			yield return new WaitForSeconds (0.5f);
		}

		//PlayAnimation (5, "zacks_door_open"); // also contains the trigger inside
		QueueTutorial("tutorial_text_weapon_fire", 3f);
		QueueTutorial("tutorial_text_weapon_reload", 3f);
		GetEntity (14).SetActive (true); // the 'leave house' trigger
		GetEntity (15).transform.eulerAngles = new Vector3(0f, 55f, 0f);
	}

	IEnumerator Story_Zack_House_6() {
		OnStoryStart ("Story_Zack_House_6");

		PlayAnimation (1, "taxi_driver_die_static"); // put the anims back in place
		PlayAnimation (4, "desmond_fall_static"); // put the anims back in place
		GetEntity (5).SetActive (false); // hide zack, just to be sure
		GetEntity (9).SetActive (false); // the light
		GetEntity (7).SetActive (true); // the roof
		TeleportEntity(player, GetEntity(1).transform.position);
		PlayCutscene ("zacks_house_22");

		PlaySound (27, 0.5f); // phone call
		yield return new WaitForSeconds (3f);
		PlayCutscene ("zacks_house_23");
		yield return new WaitForSeconds (15f);
		PlayCutscene ("zacks_house_24");
		yield return new WaitForSeconds (6.5f);
		uiManager.ShowBlackScreen ();
		yield return new WaitForSeconds (5.5f);
		uiManager.ShowBlackLoadingScreen ();
		SceneManager.LoadScene ("(G4) Urban Road");
	}


	// URBAN ROAD


	IEnumerator Story_Urban_Road_0() {
		OnStoryStart ("Story_Urban_Road_0");

		// First story of this episode
		PlayerPrefs.SetInt("currentEpisodeNumber", 1);

		PlayCutscene ("urban_road_2");
		PlayAnimation (0, "frank_walk_down_urban_road");
		PlayAnimation (1, "zack_walk_down_urban_road");
		PlaySound (0, 1f);
		yield return new WaitForSeconds (6.5f);
		PlayCutscene ("urban_road_1");
		yield return new WaitForSeconds (1.5f);

		// choice start
		choiceManager.SetupChoiceSequence(3.5f, 2, "choice_zack_shell_be_alright", "choice_zack_i_dont_know", "choice_empty");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (2, 0f); // she'll be alright
			yield return new WaitForSeconds (6.5f);
		} else if (choice == 1) {
			PlaySound (1, 0f); // i don't know
			yield return new WaitForSeconds (4.5f);
		} else {
			yield return new WaitForSeconds (1f);
		}
		// choice end

		PlaySound (3, 0f);

		yield return new WaitForSeconds (5f);

		PlayCutscene ("urban_road_4");
		PlayAnimation (0, "frank_stare_at_store");
		PlayAnimation (1, "zack_stare_at_store");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("urban_road_3");
		yield return new WaitForSeconds (1f);
		PlaySound (4, 0f);
		yield return new WaitForSeconds (1f);
		PlayCutscene ("urban_road_5");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("urban_road_6");
		yield return new WaitForSeconds (10f);

		PlayCutscene ("urban_road_7");
		PlayAnimation (0, "frank_store_load_gun");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (1, "zack_stare_store_at_frank");
		PlaySound (5, 0.5f);
		yield return new WaitForSeconds (4f);
		PlayCutscene ("urban_road_8");
		yield return new WaitForSeconds (4f);

		// choice start
		choiceManager.SetupChoiceSequence(3.5f, 1, "choice_gun_reassure", "choice_gun_discouraging");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayCutscene ("urban_road_9");

		if (choice == 0) {
			PlayerPrefs.SetInt ("choice_reassure_zack", 1);
			PlaySound (7, 0f); // reassuring
			yield return new WaitForSeconds (3f);
			PlayCutscene ("urban_road_8");
			yield return new WaitForSeconds (2.5f);
		} else {
			PlayerPrefs.SetInt ("choice_reassure_zack", 0);
			PlaySound (6, 0f); // discouraging
			yield return new WaitForSeconds (2f);
			PlayCutscene ("urban_road_8");
			yield return new WaitForSeconds (6f);
		}
		// choice end

		StartStory ("Story_Urban_Road_1");
	}

	IEnumerator Story_Urban_Road_1() {
		OnStoryStart ("Story_Urban_Road_1");

		PlayCutscene ("urban_road_10");
		PlayAnimation (1, "zack_stare_store_at_frank_static");
		PlaySound (8, 0f); // okay then ...

		yield return new WaitForSeconds (3f);

		// continuity's sake!
		GetEntity(6).SetActive(false); // stops zombies from splashing into the camera view
		GetEntity(1).SetActive(false);
		TeleportEntity (player, GetEntity (0).transform.position);
		playerInventory.ClearInventory ();
		playerInventory.AddItemToInventory (GetEntity (2));
		PlayAnimation (1, "zack_run_away_from_frank");

		OnStoryEnd ();

		yield return new WaitForSeconds (0.5f);
		QueueTutorial("urban_road_tutorial_1", 3f);

		while (playerEntity.holding.GetComponent<Gun> ().CurrentClip == playerEntity.holding.GetComponent<Gun> ().startingClip) {
			yield return new WaitForSeconds (0.1f);
		}
		PlayCutscene ("urban_road_12");
		PlaySound (9, 0.5f); // it's working!
		GetEntity(9).tag = "Survivor";
		GetEntity(10).tag = "Untagged";
		GetEntity(3).tag = "Untagged";
		playerEntity.invincible = true;
		yield return new WaitForSeconds(0.5f);
		PlayAnimation (1, "zack_run_away_from_frank_2");
		yield return new WaitForSeconds(1f);
		GetEntity(1).SetActive(true);
		PlayCutscene ("urban_road_11");
		PlayAnimation (0, "frank_urban_road_1");
		TeleportEntity (player, GetEntity (7).transform.position); // move frank off camera
		yield return new WaitForSeconds(2.5f);
		GetEntity(1).SetActive(false);
		TeleportEntity (player, GetEntity (8).transform.position); // move frank back
		GetEntity(9).tag = "Untagged";
		GetEntity(10).tag = "Survivor";
		playerEntity.invincible = false;

		OnStoryEnd ();

		//QueueTutorial("urban_road_tutorial_2", 5f);

		// change priority for zombies
		GetEntity(4).GetComponent<Spawner>().Spawn();
		yield return new WaitForSeconds (3f);
		PlayAnimation (1, "zack_unlock_bar_door");

		yield return new WaitForSeconds (4.5f);
		PlaySound (10, 0f); // anything?

		yield return new WaitForSeconds (8f);
		PlayAnimation (1, "zack_unlock_bar_door_finished");
		PlaySound (11, 0f); // finished
		GetEntity(5).SetActive(true);
	}

	IEnumerator Story_Bar_0() {
		OnStoryStart ("Story_Bar_0");

		PlayCutscene ("bar_1");
		PlayAnimation (0, "frank_bar_1");
		PlayAnimation (1, "zack_bar_1");
		PlaySound (0, 2f);
		yield return new WaitForSeconds (7f);
		PlayCutscene ("bar_2");
		yield return new WaitForSeconds (1f);

		// choice start
		choiceManager.SetupChoiceSequence(5f, 0, "choice_name_tell", "choice_name_dont_tell");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayCutscene ("bar_3");

		if (choice == 0) {
			PlayerPrefs.SetInt ("zack_knows_frank_name", 1);
			PlaySound (1, 0f); // tell
			yield return new WaitForSeconds (1f);
			PlayCutscene ("bar_4");
			yield return new WaitForSeconds (2f);
		} else {
			PlayerPrefs.SetInt ("zack_knows_frank_name", 0);
			PlaySound (2, 0f); // don't tell
			yield return new WaitForSeconds (0.9f);
			PlayCutscene ("bar_4");
			yield return new WaitForSeconds (3.9f);
		}
		// choice end

		StartStory ("Story_Bar_1");

	}

	IEnumerator Story_Bar_1() {
		OnStoryStart ("Story_Bar_1");
		OnStoryEnd ();

		PlayerPrefs.SetInt ("key_state", 0);
		TeleportEntity (player, story.destinations [0].transform.position);
		GetEntity (0).SetActive (false);

		PlaySound (3, 0f);

		PlayAnimation (1, "zack_bar_2");
		yield return new WaitForSeconds (1.8f);
		PlayAnimation (1, "zack_bar_3");

	}

	IEnumerator Story_Bar_2() {
		OnStoryStart ("Story_Bar_2");

		GetEntity (0).SetActive (true);
		PlayCutscene ("bar_5");
		PlayAnimation (0, "frank_bar_2");
		PlayAnimation (1, "zack_bar_4");
		TeleportEntity (player, story.destinations [0].transform.position);

		soundManager.FadeSound (story.audio [8], 0.5f);
		uiManager.FadeIn (2.5f);
		yield return new WaitForSeconds (5f);
		uiManager.ShowBlackLoadingScreen ();
		LoadScene ("(G6) Alleyway");
	}

	IEnumerator Story_Alleyway_0() {
		OnStoryStart ("Story_Alleyway_0");

		uiManager.ShowBlackScreen ();
		PlaySound (6, 0.2f);
		yield return new WaitForSeconds (3f);
		uiManager.FadeOut (0.5f);

		PlayAnimation (0, "frank_alleyway_1");
		PlayAnimation (1, "zack_alleyway_1");
		PlayCutscene ("alleyway_1");
		PlaySound (0, 2f);
		yield return new WaitForSeconds (5f);
		PlayCutscene ("alleyway_2");
		yield return new WaitForSeconds (0.5f);

		// choice start
		choiceManager.SetupChoiceSequence(5f, 0, "choice_holding_up_good", "choice_holding_up_okay", "choice_holding_up_bad");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (1, 0f); // good
			yield return new WaitForSeconds (1f);
		} else if(choice == 1) {
			PlaySound (3, 0f); // okay
			yield return new WaitForSeconds (1.5f);
		} else {
			PlaySound (2, 0f); // bad
			yield return new WaitForSeconds (1f);
		}
		// choice end

		PlaySound (4, 0f);

		while (story.animators [0].transform.position.x > -7) {
			yield return new WaitForSeconds (0.5f);
		}

		PlayCutscene ("alleyway_3");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("alleyway_4");
		yield return new WaitForSeconds (7.5f);

		StartStory ("Story_Alleyway_1");
	}

	IEnumerator Story_Alleyway_1() {
		OnStoryStart ("Story_Alleyway_1");

		GetEntity (0).SetActive (false); // hide frank anim
		TeleportEntity (player, story.destinations [0].transform.position); // show player
		PlayAnimation (1, "zack_alleyway_2");
		OnStoryEnd ();

		yield return new WaitForSeconds (0.5f);

		tutorialManager.QueueTutorial ("alleyway_tutorial_1", 4f);

		while (playerStats.zombieKills < 7) {
			yield return new WaitForSeconds (1f);
		}

		StartStory ("Story_Alleyway_2");
	}

	IEnumerator Story_Alleyway_2() {
		OnStoryStart ("Story_Alleyway_2");

		PlayCutscene ("alleyway_6");
		GetEntity (0).SetActive (true); // put frank anim back
		PlayAnimation (0, "frank_alleyway_2");
		PlayAnimation (1, "zack_alleyway_3");
		TeleportEntity (player, story.destinations [0].transform.position);
		GetEntity (1).SetActive (true); //invisible wall to prevent zombies entering if level restarts?

		yield return new WaitForSeconds (1f);

		PlaySound (5, 0f);

		yield return new WaitForSeconds (2f);
		PlayAnimation (1, "zack_alleyway_4");

		PlayCutscene ("alleyway_7");
		uiManager.FadeIn (1.5f);
		yield return new WaitForSeconds (3f);
		uiManager.ShowBlackLoadingScreen ();
		LoadScene ("(G7) Store");
	}

	IEnumerator Story_Store_0() {
		OnStoryStart ("Story_Store_0");

		// First story of this episode
		PlayerPrefs.SetInt("currentEpisodeNumber", 2);

		GetEntity (0).SetActive (false);
		PlayCutscene ("nothing");
		uiManager.ShowBlackScreen ();
		PlaySound (0, 0.5f);
		yield return new WaitForSeconds (4.5f);
		uiManager.FadeOut (1);
		PlayCutscene ("store_1");
		PlayAnimation (0, "frank_store_hand_mouth");
		yield return new WaitForSeconds (3.5f);
		PlayAnimation (1, "zack_store_body");
		PlayCutscene ("store_2");
		yield return new WaitForSeconds (1.3f);
		PlayAnimation (0, "frank_store_hand_mouth_down");
		yield return new WaitForSeconds (0.5f);
		PlayCutscene ("store_3");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("store_4");
		GetEntity (0).SetActive (true);
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (0, "frank_store_turn_abby");
		yield return new WaitForSeconds (1.5f);
		PlayAnimation (1, "zack_store_run_to_abby");
		yield return new WaitForSeconds (1f);
		PlayAnimation (2, "abby_hug_zack");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("store_5");
		yield return new WaitForSeconds (2f);
		PlayAnimation (1, "zack_store_end_hug");
		yield return new WaitForSeconds (0.1f);
		PlayAnimation (2, "abby_arms_down");
		yield return new WaitForSeconds (0.7f);
		PlayAnimation (0, "frank_store_turn_body");
		PlayCutscene ("store_6");
		yield return new WaitForSeconds (2f);
		PlayAnimation (2, "abby_look_at_zack");
		PlayAnimation (1, "zack_store_look_at_abby");
		yield return new WaitForSeconds (0.4f);
		PlayCutscene ("store_9");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("store_7");
		PlayAnimation (0, "frank_store_turn_abby");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("store_8");
		yield return new WaitForSeconds (2f);
		
		// who is this guy?
		PlayCutscene ("store_10");
		PlaySound (1, 0f); // i met him a few hours ago ...
		yield return new WaitForSeconds (4f);

		// what did the player say
		int choice = PlayerPrefs.GetInt("frank_help_choice");
		if (choice == 0) {
			PlaySound (2, 0f); //He told me he needed help.
			yield return new WaitForSeconds (1.6f);
		} else if (choice == 1) {
			PlaySound (3, 0f); //He told me he was scared.
			yield return new WaitForSeconds (1.8f);
		} else {
			PlaySound (4, 0f); //He told me something wasn't right.
			yield return new WaitForSeconds (2.1f);
		}

		// does zack know his name?
		PlayCutscene ("store_5");
		int knowsName = PlayerPrefs.GetInt ("zack_knows_frank_name");
		if (knowsName == 0) {
			PlaySound (6, 0f); // doesn't know his name
			yield return new WaitForSeconds (7.5f);
		} else {
			PlaySound (5, 0f); // knows his name
			yield return new WaitForSeconds (5.5f);
		}

		PlayCutscene ("store_11");
		PlaySound (7, 0f);
		yield return new WaitForSeconds (3.2f);
		PlayAnimation (2, "abby_look_at_frank");
		PlayCutscene ("store_12");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("store_13");

		// choice start
		choiceManager.SetupChoiceSequence(5f, 0, "choice_family_idk", "choice_family_close", "choice_family_none");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayCutscene ("store_15");

		if (choice == 0) {
			PlaySound (8, 0f); // i don't know
			yield return new WaitForSeconds (0.7f);
		} else if(choice == 1) {
			PlaySound (9, 0f); // not too far away
			yield return new WaitForSeconds (1.3f);
		} else {
			PlaySound (10, 0f); // i don't have a family
			yield return new WaitForSeconds (0.8f);
		}
		// choice end

		PlayCutscene ("store_16");
		PlayAnimation (0, "frank_store_look_at_scream");
		PlayAnimation (1, "zack_store_look_at_scream");
		PlayAnimation (2, "abby_look_at_scream");
		yield return new WaitForSeconds (1f);
		PlaySound (11, 0f); // oh no, abby!
		yield return new WaitForSeconds (0.3f);
		PlayCutscene ("store_17");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("store_18");
		PlayAnimation (3, "paula_scared");
		PlayAnimation (4, "dan_approach_paula");
		yield return new WaitForSeconds (1.2f);

		PlayCutscene ("store_19");
		PlayAnimation (0, "frank_store_watch_paula");
		PlayAnimation (1, "zack_store_watch_paula");
		PlayAnimation (2, "abby_watch_paula");
		PlayAnimation (3, "paula_scared");
		PlayAnimation (4, "dan_approach_paula");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("store_20");
		yield return new WaitForSeconds (0.3f);

		// choice start
		choiceManager.SetupChoiceSequence(2f, 1, "choice_intervene", "choice_do_nothing");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlayerPrefs.SetInt ("player_shoot_dan", 1);
			GetEntity (1).GetComponent<Gun> ().FireGun (true);
		} else {
			PlayerPrefs.SetInt ("player_shoot_dan", 0);
			PlayCutscene ("store_23");
			PlayAnimation (1, "zack_store_take_gun");
			yield return new WaitForSeconds(1f);
			PlayAnimation (0, "frank_store_watch_paula_no_gun");
			yield return new WaitForSeconds(1f);
			GetEntity (2).GetComponent<Gun> ().FireGun (true);
		}
		// choice end

		PlayCutscene ("store_22");
		PlayAnimation (2, "abby_run_to_paula");
		PlayAnimation (4, "dan_die");
		yield return new WaitForSeconds (1.2f);
		PlayCutscene ("store_24");
		PlayAnimation (3, "paula_stand");
		PlaySound (12, 0f); // you shot dan!
		PlayAnimation (2, "abby_store_turn_to_group");
		yield return new WaitForSeconds (2.5f);
		PlayAnimation (0, "frank_store_explain");
		PlayAnimation (1, "zack_store_watch_paula");
		PlayAnimation (3, "paula_stand_normal");
		PlayCutscene ("store_25");
		yield return new WaitForSeconds (1.5f);
		PlayAnimation (0, "frank_store_explain_shooting");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("store_26");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("store_27");
		yield return new WaitForSeconds (0.8f);
		PlayCutscene ("store_29");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("store_28");
		yield return new WaitForSeconds (5.5f);
		PlayCutscene ("store_30");

		// choice start
		choiceManager.SetupChoiceSequence(5f, 0, "choice_dan_when", "choice_dan_sorry", "choice_dan_who");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayCutscene ("store_32");

		if (choice == 0) {
			PlaySound (14, 0f); // when did he turn?
			yield return new WaitForSeconds (2f);
			PlayAnimation (3, "paula_store_turn_to_abby");
			PlayCutscene ("store_29");
			yield return new WaitForSeconds (3f);
			PlayCutscene ("store_31");
			yield return new WaitForSeconds (4.3f);
		} else if(choice == 1) {
			PlaySound (15, 0f); // sorry..
			yield return new WaitForSeconds (2f);
			PlayAnimation (1, "zack_store_look_at_frank");
			yield return new WaitForSeconds (0.3f);
			PlayAnimation (0, "frank_store_look_at_zack");
			yield return new WaitForSeconds (0.3f);
			PlayCutscene ("store_33");
			yield return new WaitForSeconds (6f);
		} else {
			PlaySound (13, 0f); // who was dan?
			yield return new WaitForSeconds (1.2f);
			PlayCutscene ("store_29");
			PlayAnimation (3, "paula_store_turn_to_abby");
			yield return new WaitForSeconds (1.1f);
		}
		// choice end

		// reset everybody's position
		PlayAnimation (0, "frank_store_explain");
		PlayAnimation (1, "zack_store_watch_paula");
		PlayAnimation (2, "abby_store_turn_to_group");
		PlayAnimation (3, "paula_stand_normal");

		PlayCutscene ("store_34");
		PlaySound (20, 0f); // zombies growl
		yield return new WaitForSeconds (3f);

		PlaySound (16, 0f);
		PlayAnimation (0, "frank_store_look_at_zack");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("store_33");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("store_29");
		yield return new WaitForSeconds (2.5f);

		PlaySound (17, 0f);
		PlayCutscene ("store_35");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("store_36");
		PlayAnimation (2, "abby_store_turn_to_paula");
		yield return new WaitForSeconds (0.3f);
		PlayAnimation (3, "paula_store_turn_to_abby");
		yield return new WaitForSeconds (9f);
		PlayCutscene ("store_37");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("store_38");
		yield return new WaitForSeconds (1f);
		PlaySound (18, 0f);
		yield return new WaitForSeconds (0.5f);
		PlayCutscene ("store_39");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("store_40");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("store_41");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("store_42");
		PlayAnimation (1, "zack_store_watch_paula");
		PlayAnimation (0, "frank_store_explain");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("store_43");
		yield return new WaitForSeconds (4f);

		StartStory ("Story_Store_1");
	}

	IEnumerator Story_Store_1() {
		OnStoryStart ("Story_Store_1");

		GetEntity (11).SetActive (false); // hide anim Frank
		GetEntity(12).SetActive(false); // hide the fake wall
		TeleportEntity(player, GetEntity(10).transform.position); // move player to old Frank's position
		PlayAnimation (1, "zack_store_wait_at_door");
		PlayAnimation (2, "abby_store_wait_at_door");
		PlayAnimation (3, "paula_mourn");
		PlayAnimation (4, "dan_die_static");
		yield return new WaitForSeconds (0f);

		OnStoryEnd ();
	}

	IEnumerator Story_Store_2() {
		OnStoryStart ("Story_Store_2");

		GetEntity (11).SetActive (true); // show anim Frank
		GetEntity(12).SetActive(true); // show the fake wall
		GetEntity(14).SetActive(false); // hide the exit trigger; it causes some issues
		GetEntity(15).SetActive(false); // hide the exit text; it causes some issues
		PlayAnimation (0, "frank_store_wait_at_door");
		PlayAnimation (1, "zack_store_wait_at_door");
		PlayAnimation (2, "abby_store_wait_at_door");
		PlayAnimation (3, "paula_store_wait_at_door");
		PlayAnimation (4, "dan_die_static");
		TeleportEntity(player, GetEntity(13).transform.position); // hide real Frank

		PlayCutscene ("store_44");
		yield return new WaitForSeconds (0.3f);
		PlaySound (19, 0f);
		yield return new WaitForSeconds (11f);
		uiManager.FadeIn (2.5f);
		yield return new WaitForSeconds (3f);
		uiManager.ShowBlackLoadingScreen ();
		LoadScene ("(G8) Parking Area");
	}

	IEnumerator Story_Parking_Area_0() {
		OnStoryStart ("Story_Parking_Area_0");

		PlayAnimation (0, "frank_car");
		PlayAnimation (1, "zack_car");
		PlayAnimation (2, "abby_car_door");
		PlayAnimation (3, "paula_car");

		PlayCutscene ("parking_area_1");
		PlaySound (0, 0.3f);
		yield return new WaitForSeconds (4f);
		PlaySound (4, 0f);
		PlayCutscene ("parking_area_2");
		ActivateSpawners ();
		GetEntity (0).GetComponent<Story_CarLight> ().StartCycle ();
		PlaySound (1, 1f);
		yield return new WaitForSeconds (2f);
		PlayCutscene ("parking_area_3");
		yield return new WaitForSeconds (2.5f);
		PlaySound (5, 0f);
		StopSound (4);

		GetEntity (0).GetComponent<Story_CarLight> ().StopCycle ();

		StartStory ("Story_Parking_Area_1");
	}

	IEnumerator Story_Parking_Area_1() {
		OnStoryStart ("Story_Parking_Area_1");

		KillAllEnemies (true, false);
		ActivateSpawners ();
		OnStoryEnd ();
		PlayAnimation (0, "frank_car");
		PlayAnimation (1, "zack_car");
		PlayAnimation (2, "abby_car_door");
		PlayAnimation (3, "paula_car");

		TeleportEntity (player, GetEntity (1).transform.position); // tp real frank
		GetEntity(2).SetActive(false); // hide fake frank
		player.GetComponent<NavMeshAgent> ().enabled = false;

		yield return new WaitForSeconds (0.3f);
		PlaySound (2, 0f);

		Story_ZombieCar car = GetEntity (3).GetComponent<Story_ZombieCar> ();
		while (!car.Collided) {
			yield return new WaitForSeconds (0.5f);
		}

		PlaySound (3, 0f);
		yield return new WaitForSeconds (0.3f);
		uiManager.ShowBlackScreen ();
		PlayCutscene ("nothing");
		playerEntity.invincible = true;
		KillAllEnemies (true, false);
		PlaySound (6, 1.5f);
		yield return new WaitForSeconds (18f);
		uiManager.ShowBlackLoadingScreen ();
		LoadScene ("(G10) Fuel Station");
	}

	IEnumerator Story_Fuel_Station_0() {
		OnStoryStart ("Story_Fuel_Station_0");

		// toggle between suns
		GetEntity (0).SetActive (true);
		GetEntity (1).SetActive (false);

		PlayCutscene ("fuel_station_1");
		PlaySound (30, 0f); // song
		PlayAnimation (0, "frank_sit_fire");
		PlayAnimation (1, "zack_sit_fire");
		PlayAnimation (2, "abby_sit_fire");
		PlayAnimation (3, "paula_sit_fire");
		PlayAnimation (4, "ranjit_sit_fire");
		yield return new WaitForSeconds (1f);
		PlaySound (0, 0f);
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (0, "frank_fire_look_at_zack");
		PlayAnimation (2, "abby_fire_look_at_zack");
		PlayAnimation (3, "paula_fire_look_at_zack");
		PlayAnimation (4, "ranjit_fire_look_at_zack");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (1, "zack_fire_toast");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("fuel_station_2"); // show zack
		PlayAnimation (1, "zack_fire_point_to_frank_ranjit");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_3"); // show frank & ranjit
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_fire_look_at_abby_from_zack");
		PlayAnimation (4, "ranjit_fire_look_at_abby_from_zack");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_4"); // show abby
		PlayAnimation (2, "abby_fire_look_at_frank_from_zack");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("fuel_station_5"); // show frank
		PlaySound (1, 2.7f);
		yield return new WaitForSeconds (3.5f);
		PlayCutscene ("fuel_station_6"); // camera pan out
		PlayAnimation (0, "frank_fire_look_at_zack");
		PlayAnimation (1, "zack_fire_talk_to_everyone");
		PlayAnimation (2, "abby_fire_look_at_zack");
		PlayAnimation (4, "ranjit_fire_look_at_zack");
		yield return new WaitForSeconds (3f);
		uiManager.FadeIn (4f);
		yield return new WaitForSeconds (3f);
		FadeSound (30, 1f);
		PlayAnimation (2, "abby_fire_look_at_ranjit");
		yield return new WaitForSeconds (7f);

		StartStory ("Story_Fuel_Station_1");
	}

	IEnumerator Story_Fuel_Station_1() {
		OnStoryStart ("Story_Fuel_Station_1");

		// toggle between suns
		GetEntity (0).SetActive (false);
		GetEntity (1).SetActive (true);

		// hello darkness my old friend
		uiManager.ShowBlackScreen ();

		// put people in their places (literally)
		PlayAnimation (0, "frank_wake_up");
		PlayAnimation (1, "zack_sleep_by_fire");
		GetEntity (2).SetActive (false); // we don't need Abby in this scene
		PlayAnimation (3, "paula_sleep_by_fire");
		PlayAnimation (4, "ranjit_fire_cook");

		// chicken
		GetEntity(9).SetActive(true); // rotisserie
		GetEntity(10).SetActive(true); // chicken

		PlaySound (2, 0.2f);

		uiManager.FadeOut (3);
		PlayCutscene ("fuel_station_8"); // wide shot
		yield return new WaitForSeconds (7f);
		PlayCutscene ("fuel_station_9"); // show frank
		PlayAnimation(0, "frank_knees_fire");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_10"); // show ranjit
		PlayAnimation (4, "ranjit_fire_cook_shoulders");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("fuel_station_11"); // show frank
		PlayAnimation(0, "frank_knees_fire_ask");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("fuel_station_12"); // show ranjit
		yield return new WaitForSeconds (2f);
		PlayAnimation (4, "ranjit_fire_cook_explain");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("fuel_station_13"); // show ranjit
		yield return new WaitForSeconds (1.5f);
		PlaySound (3, 0f);
		yield return new WaitForSeconds (2.2f);
		PlayCutscene ("fuel_station_15"); // show ranjit
		yield return new WaitForSeconds (0.3f);
		PlayAnimation (4, "ranjit_cook_anywhere");
		yield return new WaitForSeconds (4.5f);
		PlayCutscene ("fuel_station_16"); // show ranjit
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (4, "ranjit_fire_cook_explain");
		yield return new WaitForSeconds (5.6f);
		PlayCutscene ("fuel_station_18"); // OTS ranjit
		PlayAnimation (4, "ranjit_fire_cook_explain");
		PlaySound (5, 0f); // yes i am looking for my family (ADD CHOICES IF WE IMPLEMENT VAN SCENE)
		yield return new WaitForSeconds (0.3f);
		PlayAnimation(0, "frank_knees_fire_nod");
		yield return new WaitForSeconds (2.8f);
		PlaySound (8, 0f);
		PlayCutscene ("fuel_station_19"); // OTS frank
		yield return new WaitForSeconds (1.1f);
		PlayAnimation (4, "ranjit_safe_zone");
		yield return new WaitForSeconds (4.2f);
		PlayCutscene ("fuel_station_20"); // show ranjit
		yield return new WaitForSeconds (1.3f);
		PlayAnimation (4, "ranjit_safe_zone2");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("fuel_station_16"); // show ranjit
		yield return new WaitForSeconds (1f);
		PlayAnimation (4, "ranjit_fire_cook_explain");
		yield return new WaitForSeconds (4f);
		PlaySound (9, 0f);
		yield return new WaitForSeconds (1f);
		PlayCutscene ("fuel_station_21"); // payphone
		PlayAnimation (4, "ranjit_fire_cook_payphone");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_22"); // panning
		yield return new WaitForSeconds (6f);

		StartStory ("Story_Fuel_Station_2");
	}


	IEnumerator Story_Fuel_Station_2() {
		OnStoryStart ("Story_Fuel_Station_2");

		GetEntity (0).SetActive (false);
		GetEntity (1).SetActive (true);

		// chicken
		GetEntity(9).SetActive(true); // rotisserie
		GetEntity(10).SetActive(true); // chicken

		TeleportEntity (player, GetEntity (3).transform.position);
		GetEntity (4).SetActive (false);

		// put people in their places (literally)
		PlayAnimation (1, "zack_sleep_by_fire");
		GetEntity (2).SetActive (false); // we don't need Abby in this scene
		PlayAnimation (3, "paula_sleep_by_fire");
		PlayAnimation (4, "ranjit_fire_cook");
		yield return new WaitForSeconds (0f);

		OnStoryEnd ();
	}

	IEnumerator Story_Fuel_Station_3() {
		OnStoryStart ("Story_Fuel_Station_3");

		// hide pickup text
		GetEntity(5).SetActive(false);
		GetEntity (6).SetActive (false);

		// swap player with animation
		TeleportEntity (player, GetEntity (7).transform.position);
		GetEntity (4).SetActive (true);
		PlayAnimation (0, "frank_use_payphone");

		// chicken
		GetEntity(9).SetActive(true); // rotisserie
		GetEntity(10).SetActive(false); // chicken
		GetEntity(11).SetActive(true); // zack plate
		GetEntity(12).SetActive(true); // frank plate
		GetEntity(13).SetActive(true); // paula plate

		PlayCutscene ("fuel_station_23"); // payphone

		yield return new WaitForSeconds (0.8f);
		GetEntity (8).SetActive (false); // hide the phone
		yield return new WaitForSeconds (2f);

		// choice start
		choiceManager.SetupChoiceSequence(5f, 1, "choice_payphone_message", "choice_payphone_nothing");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		PlayerPrefs.SetInt ("frank_left_message", 0);
		if (choice == 0) {
			PlayerPrefs.SetInt ("frank_left_message", 1);
			PlaySound (10, 0.5f);
			yield return new WaitForSeconds (4f);
			PlayCutscene ("fuel_station_25"); // phonecall long
			yield return new WaitForSeconds (7f);
		}

		PlayCutscene ("fuel_station_24"); // payphone hangup
		GetEntity (8).SetActive (true); // show the phone
		PlayAnimation (0, "frank_payphone_hangup");
		// choice end

		PlaySound (11, 0.5f);
		yield return new WaitForSeconds (2.5f);
		PlayAnimation (0, "frank_payphone_walk_away");
		PlayCutscene ("fuel_station_26");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("fuel_station_27");

		PlayAnimation (0, "frank_sit_fire_day");
		PlayAnimation (1, "zack_sit_fire_day");
		PlayAnimation (3, "paula_say_morning");
		PlayAnimation (4, "ranjit_sit_fire");

		PlayAnimation (0, "frank_sit_day_look_at_paula");
		yield return new WaitForSeconds (4.7f);
		PlaySound (12, 0f);
		yield return new WaitForSeconds (0.3f);
		PlayAnimation (3, "paula_wheres_abby");
		PlayCutscene ("fuel_station_28");
		yield return new WaitForSeconds (2.1f);
		PlayCutscene ("fuel_station_29");
		PlayAnimation (0, "frank_sit_day_look_at_zack");
		PlayAnimation (1, "zack_explain_where_abby_is");
		PlayAnimation (4, "ranjit_sit_fire_look_at_zack");
		yield return new WaitForSeconds (2.1f);
		PlayCutscene ("fuel_station_30");
		yield return new WaitForSeconds (3.9f);
		PlayAnimation (1, "zack_stand_up_fire");
		yield return new WaitForSeconds (0.4f);
		GetEntity (11).SetActive (false); // hide zack's plate
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_31"); // show paula
		yield return new WaitForSeconds (0.8f);
		PlayCutscene ("fuel_station_32"); // show zack
		PlayAnimation (1, "zack_fire_no");
		yield return new WaitForSeconds (3.5f);
		PlayAnimation (1, "zack_walk_from_fire");
		PlayCutscene ("fuel_station_33"); // panning shot
		yield return new WaitForSeconds (1.5f);
		PlaySound (13, 0f);
		yield return new WaitForSeconds (0.2f);
		PlayAnimation (0, "frank_sit_day_look_at_paula_from_zack");
		PlayAnimation (3, "paula_ask_how_slept");
		PlayAnimation (4, "ranjit_sit_day_look_at_paula_from_zack");
		GetEntity (15).SetActive (false); // remove the fuel station exterior
		yield return new WaitForSeconds (8.4f);
		PlayCutscene ("fuel_station_34");
		GetEntity (14).SetActive (false); // hide zack until we need him
		yield return new WaitForSeconds (1.2f);
		PlayCutscene ("fuel_station_35");
		yield return new WaitForSeconds (1f);
		uiManager.ShowBlackScreen ();
		yield return new WaitForSeconds (2f);
		PlayerPrefs.SetInt ("fs_r", 1);
		StartStory ("Story_Fuel_Station_4");
	}

	IEnumerator Story_Fuel_Station_4() {
		OnStoryStart ("Story_Fuel_Station_4");

		if (PlayerPrefs.GetInt ("fs_r") == 1) {
			LoadScene ("(G10) Fuel Station");
		}
		PlayerPrefs.SetInt ("fs_r", 0);

		uiManager.ShowBlackScreen ();
		GetEntity (15).SetActive (false); // remove the fuel station exterior

		// put everyone in their places
		GetEntity (14).SetActive (true); // show zack again
		PlayAnimation (0, "frank_enter_fuel_station");
		PlayAnimation (1, "zack_stand_by_abby_dying");
		GetEntity (2).SetActive (false); // campfire abby
		PlayAnimation (5, "abby_lie_against_till");

		uiManager.HideBlackScreen ();

		PlayCutscene ("fuel_station_36");
		PlaySound (14, 0.2f);
		yield return new WaitForSeconds (1f);
		PlayAnimation (5, "abby_dying_look_frank");
		yield return new WaitForSeconds (2.2f);
		PlayAnimation (1, "zack_abby_dying_turn_to_frank");
		yield return new WaitForSeconds (2.3f);
		PlayCutscene ("fuel_station_37");
		PlayAnimation (1, "zack_abby_dying_back_to_abby");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (5, "abby_dying_look_zack");
		yield return new WaitForSeconds (3f);
		PlayAnimation (5, "abby_dying_head_down");
		yield return new WaitForSeconds (4.5f);
		PlayCutscene ("fuel_station_38");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (1, "zack_abby_dying_react_1");
		yield return new WaitForSeconds (3.5f);
		PlayAnimation (1, "zack_abby_dying_react_2");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_39");
		yield return new WaitForSeconds (0.1f);
		PlayAnimation (5, "abby_dying_show_mark");
		PlayAnimation (3, "paula_fuel_door");
		PlayAnimation (4, "ranjit_fuel_door");
		yield return new WaitForSeconds (3.2f);
		PlayCutscene ("fuel_station_40");
		PlayAnimation (0, "frank_fuel_zack_confront");
		PlayAnimation (1, "zack_abby_dying_react_3");
		PlaySound (15, 0.2f);
		yield return new WaitForSeconds (4.5f);
		PlayCutscene ("fuel_station_41");
		PlayAnimation (1, "zack_angry_at_frank");

		int playerShot = PlayerPrefs.GetInt ("player_shoot_dan"); // 1 = true, 0 = false
		if (playerShot == 1) {
			PlaySound (16, 0f);
			yield return new WaitForSeconds (2f);
		} else {
			PlaySound (17, 0f);
			yield return new WaitForSeconds (2.5f);
		}

		PlaySound (18, 0f);
		PlayCutscene ("fuel_station_42");
		PlayAnimation (0, "frank_fuel_zack_confront_react");
		yield return new WaitForSeconds (2.2f);
		PlayAnimation (5, "abby_dying_look_zack");
		PlayCutscene ("fuel_station_43");
		yield return new WaitForSeconds (2f);
		PlayAnimation (1, "zack_stand_by_abby_dying");
		PlayAnimation (5, "abby_dying_head_down");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("fuel_station_44");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("fuel_station_45");
		PlayAnimation (3, "paula_stand_by_abby");
		yield return new WaitForSeconds (0.2f);
		PlayAnimation (3, "paula_react_abby");
		PlayAnimation (0, "frank_fuel_to_paula");
		PlayAnimation (1, "zack_fuel_to_paula");
		PlayAnimation (5, "abby_dying_to_paula");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("fuel_station_46");
		yield return new WaitForSeconds (0.2f);
		PlayAnimation (3, "paula_react_abby_2");
		yield return new WaitForSeconds (3.4f);
		PlaySound (19, 0f);
		yield return new WaitForSeconds (0.5f);
		PlayCutscene ("fuel_station_47");
		PlayAnimation (1, "zack_stand_by_abby_dying");
		PlayAnimation (0, "frank_fuel_to_abby_from_paula");
		yield return new WaitForSeconds (3.5f);
		PlayCutscene ("fuel_station_49");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("fuel_station_48");
		yield return new WaitForSeconds (5f);
		PlaySound (20, 0f);
		PlayAnimation (1, "zack_abby_dying_paula_to_frank");
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_fuel_to_zack_from_paula");
		yield return new WaitForSeconds (0.4f);
		PlayCutscene ("fuel_station_50");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("fuel_station_51");
		yield return new WaitForSeconds (1.2f);
		PlayCutscene ("fuel_station_52");
		yield return new WaitForSeconds (0.8f);
		PlayAnimation (3, "paula_fuel_leave");
		yield return new WaitForSeconds (3f);

		// hide paula and ranjit
		GetEntity (16).SetActive (false);
		GetEntity (17).SetActive (false);

		// set scene
		PlayAnimation (5, "abby_lie_against_till");
		PlaySound (21, 0f);
		PlayCutscene ("fuel_station_53");

		// remove these temp things
		PlayAnimation (0, "frank_fuel_to_zack_from_paula");

		yield return new WaitForSeconds (0.3f);
		PlayAnimation (1, "zack_fuel_explain");
		yield return new WaitForSeconds (1.7f);
		PlayCutscene ("fuel_station_54");
		PlayAnimation (0, "frank_fuel_nod");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_56");
		yield return new WaitForSeconds (0.8f);
		PlayAnimation (1, "zack_fuel_glance_to_abby");
		yield return new WaitForSeconds (2.4f);
		PlayCutscene ("fuel_station_57");
		PlayAnimation (0, "frank_fuel_to_abby_from_paula");
		PlayAnimation (5, "abby_dying_look_zack");
		PlayAnimation (1, "zack_stand_by_abby_dying");
		yield return new WaitForSeconds (1.4f);
		PlaySound (22, 0f);
		yield return new WaitForSeconds (0.8f);
		PlayCutscene ("fuel_station_58");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("fuel_station_59");
		yield return new WaitForSeconds (1f);
		PlayAnimation (1, "zack_fuel_explain_to_abby");
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_fuel_to_zack_from_paula");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("fuel_station_60");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("fuel_station_61");
		PlayAnimation (5, "abby_dying_explain");
		yield return new WaitForSeconds (5.3f);
		PlayCutscene ("fuel_station_62");
		PlaySound (23, 0f);
		PlayAnimation (1, "zack_fuel_to_frank");
		PlayAnimation (5, "abby_dying_explain_frank");
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_fuel_explain_to_abby");
		yield return new WaitForSeconds (8f);
		PlayCutscene ("fuel_station_63");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("fuel_station_64");
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_fuel_explain_to_abby_2");
		yield return new WaitForSeconds (11f);
		PlayCutscene ("fuel_station_65");
		yield return new WaitForSeconds (0.4f);
		PlayAnimation (0, "frank_explain_to_abby_3");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("fuel_station_66");
		yield return new WaitForSeconds (0.3f);

		// choice start
		choiceManager.SetupChoiceSequence(-1, 0, "choice_zack_accept", "choice_zack_decline", "choice_zack_compromise");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		// compromise
		if (choice == 2) {
			PlayCutscene ("fuel_station_67");
			PlaySound (26, 0f);
			yield return new WaitForSeconds(0.6f);
			PlayAnimation (0, "frank_leave_her_here");
			yield return new WaitForSeconds(3f);
			PlayCutscene ("fuel_station_68");
			PlayAnimation (1, "zack_no_compromise");
			yield return new WaitForSeconds(7f);

			PlayCutscene ("fuel_station_66");
			yield return new WaitForSeconds (0.3f);
			choiceManager.SetupChoiceSequence(-1, 0, "choice_zack_accept", "choice_zack_decline");
			while(!choiceManager.ChoiceIsMade ()) {
				yield return new WaitForSeconds(0.1f);
			}
			choice = choiceManager.GetChoice ();
		}

		// accept
		if (choice == 0) {
			PlayerPrefs.SetInt ("shot_zacks_wife", 1);
			PlaySound (24, 0f);
			PlayAnimation (0, "frank_ill_do_it");
			PlayCutscene ("fuel_station_69");
			yield return new WaitForSeconds (3f);
			PlayAnimation (0, "frank_ill_do_it_2");
			yield return new WaitForSeconds (3.5f);
			PlayCutscene ("fuel_station_70");
			yield return new WaitForSeconds (4f);
			PlaySound (31, 0f);
			yield return new WaitForSeconds (0.1f);
			uiManager.ShowBlackScreen ();
			yield return new WaitForSeconds (3f);
		} 

		// decline
		if (choice == 1) {
			PlayerPrefs.SetInt ("shot_zacks_wife", 0);
			PlaySound (25, 0f);
			PlayAnimation (0, "frank_refuse_kill_abby");
			PlayCutscene ("fuel_station_71");
			yield return new WaitForSeconds (8f);
			PlayCutscene ("fuel_station_72");
			yield return new WaitForSeconds (6f);
			PlayAnimation (1, "zack_abby_dying_back_to_abby");
			PlayCutscene ("fuel_station_73");
			yield return new WaitForSeconds (1.7f);
			PlayCutscene ("fuel_station_70");
			PlayAnimation (0, "frank_fuel_station_leave");
			yield return new WaitForSeconds (3.6f);
			PlaySound (31, 0f);
			yield return new WaitForSeconds (0.1f);
			uiManager.FadeIn (2f);
			yield return new WaitForSeconds (3f);
		}

		// choice end
		uiManager.ShowBlackLoadingScreen();
		LoadScene ("(G11) Train Station");

	}

	IEnumerator Story_Train_Station_0() {
		OnStoryStart ("Story_Train_Station_0");

		// First story of this episode
		PlayerPrefs.SetInt("currentEpisodeNumber", 3);

		TrainRoadInfinity road = GetEntity (0).GetComponent<TrainRoadInfinity> ();
		road.Move ();

		GetEntity (1).SetActive (false); // hide the real station
		PlayAnimation (0, "frank_walk_down_road");
		PlayAnimation (1, "zack_walk_down_road");
		PlayAnimation (2, "paula_walk_down_road");
		PlayAnimation (3, "ranjit_walk_down_road");
		PlayCutscene ("train_station_1");

		yield return new WaitForSeconds (3f);
		PlaySound (0, 0);
		yield return new WaitForSeconds (8f);

		PlayCutscene ("train_station_2");

		// choice start
		choiceManager.SetupChoiceSequence(5f, 2, "choice_i_understand", "choice_you_arent_alone", "choice_empty");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (1, 0f); // i understand
			yield return new WaitForSeconds(1f);
			PlayCutscene ("train_station_3");
			yield return new WaitForSeconds (6f);
		} else if(choice == 1) {
			PlaySound (2, 0f); // you aren't alone
			yield return new WaitForSeconds(1f);
			PlayCutscene ("train_station_3");
			yield return new WaitForSeconds (9.2f);
		}
		// choice end

		PlaySound (3, 0);
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_station_4");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("train_station_5");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("train_station_3");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("train_station_6");
		yield return new WaitForSeconds (12f);
		PlayCutscene ("train_station_5");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("train_station_7");

		StartStory ("Story_Train_Station_1");
	}

	IEnumerator Story_Train_Station_1() {
		OnStoryStart ("Story_Train_Station_1");

		PlaySound (4, 4f);
		GetEntity (0).SetActive (false); // hide the walking part
		GetEntity (1).SetActive (true); // show the real station
		ActivateSpawners ();
		PlayCutscene ("train_station_8");
		PlayAnimation (0, "frank_outside_station");
		PlayAnimation (1, "zack_outside_station");
		PlayAnimation (2, "paula_outside_station");
		PlayAnimation (3, "ranjit_outside_station");
		yield return new WaitForSeconds (8.5f);
		PlayCutscene ("train_station_11");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_station_9");
		yield return new WaitForSeconds (3.5f);
		PlaySound (5, 0f);
		yield return new WaitForSeconds (0.9f);
		PlayCutscene ("train_station_10");
		PlayAnimation (3, "ranjit_jesture_at_frank");
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_look_at_ranjit");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_station_12");
		yield return new WaitForSeconds (0.4f);
		PlaySound (6, 0f);
		PlayAnimation (3, "ranjit_gesture_zack_paula");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (0, "frank_look_at_zack_paula");
		PlayAnimation (2, "paula_look_at_ranjit");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("train_station_13");
		yield return new WaitForSeconds (3f);
		PlayAnimation (3, "ranjit_outside_station");
		PlayCutscene ("train_station_11");
		yield return new WaitForSeconds (2.6f);
		PlayAnimation (2, "paula_look_at_zack");
		yield return new WaitForSeconds (0.4f);
		PlayCutscene ("train_station_14");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("train_station_13");
		PlayAnimation (1, "zack_cowardice");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("train_station_15");
		PlayAnimation (1, "zack_outside_station");
		yield return new WaitForSeconds (4f);
		PlayAnimation (2, "paula_zack_to_group");
		PlayCutscene ("train_station_11");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("train_station_10");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (0, "frank_look_at_ranjit");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("train_station_12");
		PlayAnimation (0, "frank_look_at_zack_paula");
		yield return new WaitForSeconds (7f);
		PlayCutscene ("train_station_16");
		yield return new WaitForSeconds (5.5f);
		PlayCutscene ("train_station_10");

		StartStory ("Story_Train_Station_2");
	}

	IEnumerator Story_Train_Station_2() {
		OnStoryStart ("Story_Train_Station_2");

		GetEntity (0).SetActive (false); // hide the walking part
		GetEntity (1).SetActive (true); // show the real station
		ActivateSpawners ();
		PlayCutscene ("train_station_10");
		PlayAnimation (0, "frank_outside_station");
		PlayAnimation (3, "ranjit_outside_station");
		GetEntity (2).SetActive (false);
		GetEntity (3).SetActive (false); // bye zack & paula
		PlaySound (7, 0f);
		yield return new WaitForSeconds (0.2f);
		PlayAnimation (3, "ranjit_look_at_frank");
		yield return new WaitForSeconds (0.9f);
		PlayAnimation (0, "frank_look_at_ranjit");

		yield return new WaitForSeconds (1f);

		OnStoryEnd ();

		// Set up Ranjit
		EnableEntityShooting(5);


		TeleportEntity (player, GetEntity (4).transform.position);
		GetEntity (4).SetActive (false);

		while (playerStats.zombieKills < 35) {
			yield return new WaitForSeconds (1f);
		}

		KillAllEnemies (true, false);
		StartStory ("Story_Train_Station_3");
	}

	IEnumerator Story_Train_Station_3() {
		OnStoryStart ("Story_Train_Station_3");

		// Fix Ranjit
		DisableEntityShooting (5);

		GetEntity (0).SetActive (false); // hide the walking part
		GetEntity (1).SetActive (true); // show the real station
		GetEntity (2).SetActive (true);
		GetEntity (3).SetActive (true);
		GetEntity (4).SetActive (true);
		GetEntity (5).SetActive (true);
		TeleportEntity (player, GetEntity (7).transform.position);

		PlayCutscene ("train_station_17");

		PlayAnimation (0, "frank_outside_train");
		PlayAnimation (1, "zack_outside_train");
		PlayAnimation (2, "paula_outside_train");
		PlayAnimation (3, "ranjit_outside_train");
		PlaySound (8, 1f);

		yield return new WaitForSeconds(7f);

		PlayCutscene ("train_station_18");
		yield return new WaitForSeconds(3f);
		GetEntity (6).SetActive (true); // fake zombie
		PlayAnimation(4, "fake_zombie_train");
		PlayCutscene ("train_station_19");
		yield return new WaitForSeconds(3f);
		PlayCutscene ("train_station_20");
		GetEntity (6).SetActive (false); // fake zombie
		yield return new WaitForSeconds(2.5f);
		PlayCutscene ("train_station_21");
		yield return new WaitForSeconds(3f);
		PlayCutscene ("train_station_18");
		yield return new WaitForSeconds(3.5f);

		StartStory ("Story_Train_Station_4");
	}

	IEnumerator Story_Train_Station_4() {
		OnStoryStart ("Story_Train_Station_4");

		GetEntity (0).SetActive (false); // hide the walking part
		GetEntity (1).SetActive (true); // show the real station
		GetEntity (2).SetActive (true);
		GetEntity (3).SetActive (true);
		GetEntity (4).SetActive (true);
		GetEntity (5).SetActive (true);
		TeleportEntity (player, GetEntity (1).transform.position);

		PlayCutscene ("train_station_22");

		PlayAnimation (0, "frank_inside_train");
		PlayAnimation (1, "zack_inside_train");
		PlayAnimation (2, "paula_inside_train");
		PlayAnimation (3, "ranjit_inside_train");
		PlaySound (9, 1f);

		yield return new WaitForSeconds (5.1f);

		PlayAnimation (5, "train_doors_new");
		PlaySound (10, 0);
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_station_23");
		yield return new WaitForSeconds (2f);
		PlaySound (11, 0);

		TrainRoadInfinity moving = GetEntity (1).GetComponent<TrainRoadInfinity> ();
		moving.Move ();
		moving.SpeedUp (1);

		PlaySound (12, 0);
		PlayAnimation (0, "frank_thrown_back");
		PlayAnimation (2, "paula_thrown_back");
		PlayAnimation (1, "zack_thrown_back");
		yield return new WaitForSeconds (0.4f);
		PlayCutscene ("train_station_26");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_station_24");
		yield return new WaitForSeconds (2f);
		PlayAnimation (3, "ranjit_look_in_train");
		PlayCutscene ("train_station_25");
		yield return new WaitForSeconds (2.4f);
		PlayCutscene ("train_station_27");
		yield return new WaitForSeconds (2f);
		uiManager.ShowBlackLoadingScreen ();
		LoadScene ("(G12) Train");
	}

	IEnumerator Story_Train_0() {
		OnStoryStart ("Story_Train_0");

		PlayAnimation (1, "zack_back_of_train");
		PlayAnimation (2, "paula_back_of_train");
		PlayAnimation (3, "ranjit_back_of_train");

		OnStoryEnd ();
		yield return new WaitForSeconds (0f);
	}

	IEnumerator Story_Train_1() {
		OnStoryStart ("Story_Train_1");
		TrainModules modules = GetEntity (22).GetComponent<TrainModules> ();

		uiManager.ShowBlackScreen ();
		TeleportEntity (player, GetEntity (23).transform.position);
		yield return new WaitForSeconds (1f);
		KillAllEnemies (true, false);
		uiManager.HideBlackScreen ();

		for (int i = 0; i < 21; i++) {
			GetEntity (i).GetComponent<ParticleSystem> ().Play ();
		}

		PlayCutscene ("train_1");
		PlayAnimation (0, "frank_train_cabin");
		PlayAnimation (1, "zack_train_cabin");
		PlayAnimation (2, "paula_train_cabin");
		PlayAnimation (3, "ranjit_cabin");
		PlaySound (0, 0.2f);

		yield return new WaitForSeconds (2f);
		PlayAnimation (3, "ranjit_throw_zombie");
		yield return new WaitForSeconds (1f);
		PlaySound (1, 0);
		modules.SlowDown ();
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_2");
		yield return new WaitForSeconds (0.4f);
		PlaySound (2, 0);
		yield return new WaitForSeconds (2.2f);
		PlayCutscene ("train_3");
		for (int i = 0; i < 21; i++) {
			Destroy (GetEntity (i));
		}
		yield return new WaitForSeconds (1f);
		PlayAnimation (3, "ranjit_drive_train");
		PlaySound (3, 0);
		modules.SpeedUp ();
		yield return new WaitForSeconds (0.3f);
		PlayCutscene ("train_4");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("train_2");
		yield return new WaitForSeconds (1f);
		PlaySound (4, 0);
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_5");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("train_6");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("train_1");
		yield return new WaitForSeconds (2.8f);
		PlayCutscene ("train_7");
		yield return new WaitForSeconds (2.2f);
		PlayCutscene ("train_8");
		PlayAnimation (0, "frank_train_turn_paula");
		yield return new WaitForSeconds (0.8f);
		PlayAnimation (2, "paula_train_to_zack");
		yield return new WaitForSeconds (0.4f);
		PlayAnimation (1, "zack_train_turn_paula");
		yield return new WaitForSeconds (0.6f);
		PlayAnimation (2, "paula_train_gesture");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("train_9");
		yield return new WaitForSeconds (6f);
		PlayCutscene ("train_10");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("train_11");
		yield return new WaitForSeconds (0.2f);
		PlayAnimation (1, "zack_train_turn_frank");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("train_12");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("train_9");
		yield return new WaitForSeconds (1.3f);
		PlayCutscene ("train_13");
		yield return new WaitForSeconds (1.2f);

		uiManager.ShowBlackScreen ();
		yield return new WaitForSeconds (2f);
		uiManager.ShowBlackLoadingScreen ();
		LoadScene ("(G13) Train Station 2");
	}

	IEnumerator Story_Train_Station_2_0() {
		OnStoryStart ("Story_Train_Station_2_0");

		// First story of this episode
		PlayerPrefs.SetInt("currentEpisodeNumber", 4);

		ActivateSpawners ();

		PlayCutscene ("new_train_station_1");
		PlayAnimation (0, "frank_train_station_2");
		PlayAnimation (1, "zack_train_station_2");
		PlayAnimation (2, "paula_train_station_2");
		PlayAnimation (3, "ranjit_train_station_2");

		PlaySound (0, 0.5f);

		yield return new WaitForSeconds (6f);
		PlayCutscene ("new_train_station_2");
		yield return new WaitForSeconds (1.4f);
		PlayCutscene ("new_train_station_3");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("new_train_station_4");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("new_train_station_5");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("new_train_station_6");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("new_train_station_7");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("new_train_station_9");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("new_train_station_10");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("new_train_station_8");
		PlaySound (7, 0);
		GetEntity (0).SetActive(true);
		GetEntity (0).GetComponent<Story_TrainMove> ().Move ();
		yield return new WaitForSeconds (1.3f);
		PlayAnimation (0, "frank_train_station_2_wave");
		PlayAnimation (1, "zack_train_station_2_wave");
		yield return new WaitForSeconds (1f);
		GetEntity (1).SetActive (false);
		GetEntity (2).SetActive (false);
		yield return new WaitForSeconds (8f);
		PlayCutscene ("new_train_station_11");
		PlayAnimation (0, "frank_train_station_2_2");
		PlayAnimation (1, "zack_train_station_2_2");
		GetEntity (0).SetActive (false);
		GetEntity (3).SetActive (false);
		PlaySound (1, 1.4f);
		yield return new WaitForSeconds (1f);
		PlayAnimation (0, "frank_train_station_2_react");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("new_train_station_12");
		yield return new WaitForSeconds (5.6f);

		// choice start
		choiceManager.SetupChoiceSequence(4f, 0, "choice_dont_prove_it", "choice_prove_it");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();
		PlayCutscene ("new_train_station_13");

		if (choice == 0) {
			PlaySound (2, 0f); // dont prove it
			yield return new WaitForSeconds (6.5f);
		} else {
			PlaySound (3, 0f); // prove it
			yield return new WaitForSeconds (1.2f);
		}
		// choice end

		StartStory ("Story_Train_Station_2_1");
	}

	IEnumerator Story_Train_Station_2_1() {
		OnStoryStart ("Story_Train_Station_2_1");

		ActivateSpawners ();

		// get rid of the train
		GetEntity(0).SetActive(false);
		GetEntity (3).SetActive (false);

		PlaySound (4, 0f);
		PlayCutscene ("new_train_station_14");
		PlayAnimation (0, "frank_train_station_2_shoot");
		PlayAnimation (1, "zack_train_station_2_shoot");
		yield return new WaitForSeconds(2f);
		GetEntity (4).SetActive (false);
		EnableEntityShooting(5);
		Destroy (GetEntity (6));
		GetEntity (5).transform.Find ("Character").transform.localEulerAngles = Vector3.zero;
		OnStoryEnd ();
		TeleportEntity (player, GetEntity (7).transform.position);
		GetEntity (7).SetActive (false);

		while (playerStats.zombieKills < 68) {
			yield return new WaitForSeconds (1f);
		}

		PlayerPrefs.SetInt ("tra_r_s", 1);
		StartStory ("Story_Train_Station_2_2");
	}

	IEnumerator Story_Train_Station_2_2() {
		OnStoryStart ("Story_Train_Station_2_2");

		if (PlayerPrefs.GetInt ("tra_r_s") == 1)
			SceneManager.LoadScene ("(G13) Train Station 2");
		PlayerPrefs.SetInt ("tra_r_s", 0);

		KillAllEnemies (true, false);
		DisableEntityShooting (5);
		GetEntity (5).SetActive (true); // zack
		GetEntity (7).SetActive (true); // frank
		GetEntity (8).SetActive (false); // zack weapon holder

		PlaySound (5, 1f);
		PlayCutscene ("new_train_station_15");
		PlayAnimation (0, "frank_train_station_2_center");
		PlayAnimation (1, "zack_train_station_2_center");

		// get rid of the train
		GetEntity(0).SetActive(false);
		GetEntity (3).SetActive (false);

		yield return new WaitForSeconds (5f);
		PlaySound (6, 0);
		yield return new WaitForSeconds (4f);
		PlayAnimation (0, "frank_train_station_2_walk");
		yield return new WaitForSeconds (0.5f);
		PlayAnimation (1, "zack_train_station_2_walk");
		yield return new WaitForSeconds (0.5f);
		PlayCutscene ("new_train_station_16");
		uiManager.FadeIn (3f);

		yield return new WaitForSeconds (3f);

		LoadScene ("(G14) Franks House");
	}

	IEnumerator Story_Franks_House_0() {
		OnStoryStart ("Story_Franks_House_0");

		PlaySound (0, 0.3f);
		PlayCutscene ("franks_house_1");
		PlayAnimation (0, "frank_house_enter");
		yield return new WaitForSeconds (1f);
		PlayAnimation (1, "zack_enter_franks_house");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("franks_house_2");
		PlayAnimation (1, "zack_franks_house_explain_entrance");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("franks_house_3");
		PlayAnimation (1, "zack_franks_house_explore");
		PlayAnimation (0, "frank_house_step_back");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("franks_house_4");
		yield return new WaitForSeconds (1.5f);
		PlaySound (1, 0f);
		yield return new WaitForSeconds (1f);
		PlayCutscene ("franks_house_5");
		PlayAnimation (0, "frank_house_phone");
		yield return new WaitForSeconds (7.5f);
		PlayCutscene ("franks_house_6");
		yield return new WaitForSeconds (7.5f);
		PlayCutscene ("franks_house_7");
		yield return new WaitForSeconds (9.8f);
		PlayCutscene ("franks_house_8");

		int finalMessage = PlayerPrefs.GetInt ("frank_left_message");
		finalMessage = 1;
		if (finalMessage == 0) {
			PlaySound (3, 0f);
			yield return new WaitForSeconds (6f);
		} else {
			PlaySound (2, 0f);
			yield return new WaitForSeconds (12f);
		}

		PlayCutscene ("franks_house_9");
		PlaySound (4, 0f);
		yield return new WaitForSeconds (0.6f);
		PlayAnimation (0, "frank_house_phone_turn");
		yield return new WaitForSeconds (1.6f);

		PlaySound (5, 0f);
		PlayCutscene ("franks_house_10");
		PlayAnimation (1, "zack_franks_house_stand_by_sofa");
		PlayAnimation (0, "frank_house_approach_body");
		yield return new WaitForSeconds (2.3f);
		PlayAnimation (0, "frank_house_kneel_sofa");
		yield return new WaitForSeconds (7f);
		PlayCutscene ("franks_house_11");
		yield return new WaitForSeconds (3.3f);
		PlaySound (6, 0f);
		yield return new WaitForSeconds (5f);
		PlayCutscene ("franks_house_12");
		yield return new WaitForSeconds (5f);
		PlaySound (7, 0f);
		yield return new WaitForSeconds (6f);
		PlayCutscene ("franks_house_13");
		PlayAnimation (1, "zack_franks_house_im");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("franks_house_14");
		PlayAnimation (0, "frank_house_cry_sofa");
		yield return new WaitForSeconds (0.3f);
		PlayAnimation (0, "frank_house_cry_sofa_anim");
		yield return new WaitForSeconds (0.9f);
		PlayCutscene ("franks_house_15");
		yield return new WaitForSeconds (2.3f);
		PlaySound (8, 0f);
		PlayCutscene ("franks_house_16");
		PlayAnimation (1, "zack_franks_house_sofa_sorry");
		yield return new WaitForSeconds (7.1f);
		PlayCutscene ("franks_house_17");
		PlayAnimation (0, "frank_house_sit_by_couch_react");
		yield return new WaitForSeconds (8.5f);
		PlayCutscene ("franks_house_18");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("franks_house_19");
		PlayAnimation (0, "frank_house_sit_by_couch_react_2");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("franks_house_20");
		PlayAnimation (1, "zack_franks_house_i_heard_that");
		yield return new WaitForSeconds (2.5f);
		PlayCutscene ("franks_house_21");
		PlayAnimation (0, "frank_house_realise");
		yield return new WaitForSeconds (3f);
		uiManager.ShowBlackScreen ();
		yield return new WaitForSeconds (2f);

		StartStory ("Story_Franks_House_1");
	}

	IEnumerator Story_Franks_House_1() {
		OnStoryStart ("Story_Franks_House_1");

		uiManager.ShowBlackScreen ();
		PlayCutscene ("nothing");

		PlaySound (9, 0.5f);
		yield return new WaitForSeconds (1f);
		uiManager.HideBlackScreen ();
		PlayAnimation (0, "frank_enter_cellar");
		PlayAnimation (1, "zack_enter_cellar");
		PlayAnimation (2, "mary_sit_chair");
		PlayAnimation (3, "jason_look_at_frank_1");
		PlayCutscene ("franks_house_22");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("franks_house_23");
		PlayAnimation (2, "mary_look_at_frank_1");
		yield return new WaitForSeconds (2.4f);
		PlayAnimation (2, "mary_look_at_frank_2");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("franks_house_24");
		PlayAnimation (0, "frank_cellar_approach");
		yield return new WaitForSeconds (2f);
		PlayAnimation (2, "mary_look_at_frank_3");
		PlayCutscene ("franks_house_25");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("franks_house_26");
		PlayAnimation (0, "frank_she_is_sick");
		yield return new WaitForSeconds (1.7f);
		PlayCutscene ("franks_house_24");
		yield return new WaitForSeconds (2.8f);
		PlayAnimation (0, "frank_cellar_turn_to_mary");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("franks_house_27");
		PlayAnimation (2, "mary_try_hug");
		yield return new WaitForSeconds (1.4f);
		PlayAnimation (0, "frank_cellar_react");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("franks_house_29");
		PlayAnimation (0, "frank_cellar_react_2");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("franks_house_28");
		PlayAnimation (3, "jason_head_down");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("franks_house_30");
		yield return new WaitForSeconds (4f);

		PlayCutscene ("franks_house_31");
		PlayAnimation (1, "zack_cellar_when_bit");
		PlaySound (10, 0f);
		yield return new WaitForSeconds (2f);
		PlayCutscene ("franks_house_28");
		PlayAnimation (3, "jason_look_zack_static");
		yield return new WaitForSeconds (1.2f);
		PlayCutscene ("franks_house_32");
		yield return new WaitForSeconds (3f);
		PlayCutscene ("franks_house_33");
		PlayAnimation (0, "frank_cellar_hes_lying");
		yield return new WaitForSeconds (4.7f);
		PlayCutscene ("franks_house_28");
		PlayAnimation (3, "jason_look_zack_static");
		yield return new WaitForSeconds (1.2f);
		PlayCutscene ("franks_house_34");
		yield return new WaitForSeconds (8.5f);
		PlayCutscene ("franks_house_35");
		yield return new WaitForSeconds (6.5f);
		PlayCutscene ("franks_house_36");
		PlayAnimation (0, "frank_cellar_hes_lying_2");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("franks_house_37");

		PlaySound (11, 0f);
		PlayAnimation (3, "jason_zack_to_mary");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("franks_house_38");
		yield return new WaitForSeconds (4f);
		PlayAnimation (3, "jason_mary_to_frank");
		yield return new WaitForSeconds (1f);
		PlayCutscene ("franks_house_39");
		PlayAnimation (0, "frank_cellar_we_can_fix_this");
		yield return new WaitForSeconds (4f);
		PlayAnimation (3, "jason_zack_to_mary");
		PlayCutscene ("franks_house_40");
		yield return new WaitForSeconds (6.5f);
		PlayCutscene ("franks_house_41");
		yield return new WaitForSeconds (6.5f);

		PlaySound (12, 0f);
		PlayCutscene ("franks_house_39");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("franks_house_40");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("franks_house_42");
		yield return new WaitForSeconds (5f);
		PlayAnimation (0, "frank_cellar_refuse");
		PlayCutscene ("franks_house_39");
		yield return new WaitForSeconds (5f);

		PlaySound (13, 0f);
		PlayCutscene ("franks_house_43");
		yield return new WaitForSeconds (5f);
		PlayCutscene ("franks_house_40");
		yield return new WaitForSeconds (6.5f);

		PlaySound (14, 0f);
		PlayAnimation (3, "jason_mary_to_frank");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("franks_house_46");
		yield return new WaitForSeconds (1.5f);
		PlayCutscene ("franks_house_47");
		yield return new WaitForSeconds (4f);
		PlayCutscene ("franks_house_35");
		yield return new WaitForSeconds (7.4f);
		PlayAnimation (0, "frank_cellar_turn_to_mary");
		PlayAnimation (3, "jason_zack_to_mary");

		int choice = PlayerPrefs.GetInt ("shot_zacks_wife");
		if (choice == 0) {
			// didnt shoot
			PlaySound (15, 0f);
			yield return new WaitForSeconds (2f);
		} else {
			// shot
			PlaySound (16, 0f);
			yield return new WaitForSeconds (1.5f);
		}
		PlaySound (17, 0f);
		yield return new WaitForSeconds (0.5f);
		PlayCutscene ("franks_house_48");
		yield return new WaitForSeconds (2f);
		PlayCutscene ("franks_house_49");

		// choice start
		choiceManager.SetupChoiceSequence(-1f, 0, "choice_do_it", "choice_zack_please");
		while(!choiceManager.ChoiceIsMade ()) {
			yield return new WaitForSeconds(0.1f);
		}
		choice = choiceManager.GetChoice ();

		if (choice == 0) {
			PlaySound (18, 0f); // frank do it
			PlayCutscene ("franks_house_50");
			PlayAnimation (0, "frank_cellar_decision_b");
			PlayerPrefs.SetInt ("killed_mary", 1);
			yield return new WaitForSeconds (4.3f);
		} else {
			PlaySound (19, 0f); // zack do it
			PlayCutscene ("franks_house_51");
			PlayAnimation (0, "frank_cellar_decision");
			PlayerPrefs.SetInt ("killed_mary", 0);
			yield return new WaitForSeconds (5f);
		}
		// choice end

		PlayAnimation (0, "frank_cellar_refuse");
		PlayAnimation (1, "zack_cellar_when_bit");
		PlayAnimation (2, "mary_try_hug");
		PlayAnimation (3, "jason_zack_to_mary");

		PlaySound (20, 0f);
		PlayAnimation (0, "frank_cellar_turn_to_mary");
		PlayAnimation (3, "jason_zack_to_mary");
		PlayCutscene ("franks_house_40");
		yield return new WaitForSeconds (6.5f);
		PlayCutscene ("franks_house_41");
		yield return new WaitForSeconds (6.5f);
		PlayCutscene ("franks_house_52");
		PlaySound (21, 0f);

		yield return new WaitForSeconds (8f);
		uiManager.ShowBlackScreen ();
		yield return new WaitForSeconds (5f);
		PlaySound (23, 0f);
		yield return new WaitForSeconds (3f);
		PlaySound (22, 0f);
		yield return new WaitForSeconds (9f);

		PlayerPrefs.SetInt ("loadToResults", 1);
		PlayerPrefs.SetInt ("uploadResults", 1);
		LoadScene ("(UI) Credits");
	}

}