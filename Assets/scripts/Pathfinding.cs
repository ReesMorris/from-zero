using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Pathfinding : MonoBehaviour {

	[Header("Targeting")]
	public bool approachTarget = true;
	public bool useRange = true;
	public bool removeTargetOnReach = false;
	public GameObject target;

	[Header("Active Search")]
	public bool seekTag;
	public enum tags{Survivor, Zombie}; // [4] Prevents issues of incorrect spelling of tag name(s)
	public tags tagToSeek;
	public float searchRangeEmpty;
	public float searchRangeFound; // Allow the option to search a larger radius if the pathfinding already has a target

	[Header("Wander")]
	public bool roamWhenNoTarget = false;
	public float wanderTimerMax;
	public float wanderRadius;
	public float wanderSpeed;

	[Header("Sound")]
	public AudioSource walking;

	private NavMeshAgent navMesh;
	private GameObject[] targets;
	private Rigidbody rb;
	private SoundManager soundManager;

	// Used for finding closest target
	private int shortestIndex;
	private float shortestDist;
	private float dist;

	// Wandering
	private float wander;
	private float wanderTimer;
	private float defaultSpeed;

	void Start () {
		// Get the entity's navmesh
		navMesh = GetComponent<NavMeshAgent> ();
		rb = GetComponent<Rigidbody> ();
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();

		defaultSpeed = navMesh.speed;

		// Prevent unnecessary extra code (enemy will stop searching then immediately find the target again)
		if (searchRangeFound < searchRangeEmpty) {
			searchRangeFound = searchRangeEmpty;
			Debug.LogError (gameObject.name + ": searchRangeFound should not be less than searchRangeEmpty.");
		}
	}

	// Update is called once per frame
	void Update () {
		if (navMesh.enabled) {
			if (target != null && approachTarget) {
				if (removeTargetOnReach && Mathf.Floor(target.transform.position.x) == Mathf.Floor(transform.position.x) && Mathf.Floor(target.transform.position.z) == Mathf.Floor(transform.position.z)) {
					StopSeeking ();
				} else {
					// Check that the target is still in range
					if (TargetIsInRange () || !useRange) {
						// In range; keep going

						navMesh.speed = defaultSpeed;
						rb.isKinematic = false;
						navMesh.destination = target.transform.position;
						navMesh.isStopped = false;

						// Play walking sound
						if (walking != null && !walking.isPlaying) {
							soundManager.PlaySound (walking, 0f);
						}
					} else {
						// Out of range; remove target and stop the AI moving towards
						StopSeeking ();
					}
				}
			} else {
				StopSeeking ();
				if (seekTag) {
					// If the entity should find other entities with a specific tag, find one
					targets = GameObject.FindGameObjectsWithTag (tagToSeek.ToString ());
					GameObject newTarget = FindNearestTarget ();

					// If a target has been found within range, let's update it
					if (newTarget != null) {
						target = newTarget;
					}
				}
			}
		}

		if (target == null && roamWhenNoTarget) {
			Roam ();
		}

		if (target != null) {
			if (target.tag.ToString() != tagToSeek.ToString()) {
				StopSeeking ();
			}
		}
	}

	// Returns either a GameObject or null depending on if an entity with a specific tag is found in range
	GameObject FindNearestTarget() {
		// If at least 1 entity with that tag exists
		if (targets.Length > 0) {
			for (int i = 0; i < targets.Length; i++) {
				// Get the distance betweeen the target entity and this gameObject
				dist = Vector3.Distance (transform.position, targets [i].transform.position);
				if (i == 0) {
					// Store some default variables we can use for comparison, otherwise everything
					// would equal 0 and no other targets could possibly be closer (even if they are)
					shortestDist = dist;
					shortestIndex = i;
				} else {
					if (dist < shortestDist && targets [i] != null) {
						// Entity is closer, let's update it
						shortestDist = dist;
						shortestIndex = i;
					}
				}
			}

			// Returns an object if: a target exists that is close enough to this object
			if (shortestDist <= searchRangeEmpty && targets [shortestIndex] != null) {
				return targets [shortestIndex];
			}
		}

		// Nothing found in range
		return null;
	}

	// Returns true if the current target is close enough
	bool TargetIsInRange() {
		if (target != null) {
			float distance = Vector3.Distance (transform.position, target.transform.position);
			if (distance <= searchRangeFound) {
				return true;
			}
		}
		return false;
	}

	// Stops the current agent from moving (may restart seeking again if no other variables are changed)
	public void StopSeeking() {
		if (!roamWhenNoTarget) {
			rb.isKinematic = true;
			navMesh.isStopped = true;
		}
		target = null;

		// Stop walking sound
		if (walking != null) {
			soundManager.StopSound (walking);
		}
	}

	// Call when there are no targets, to add some more realism to the entity
	// Source: https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/ (20 Jan 2018 :: 12:29pm)
	void Roam() {
		navMesh.speed = wanderSpeed;
		wander += Time.deltaTime;
		if (wander >= wanderTimer || navMesh.destination == Vector3.zero) {
			Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
			navMesh.SetDestination(newPos);

			wander = 0;
			wanderTimer = Mathf.Round (Random.Range (1, wanderTimerMax));
		}
	}

	// Source: https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/ (20 Jan 2018 :: 12:29pm)
	Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
		Vector3 randDirection = Random.insideUnitSphere * dist;
		randDirection += origin;
		NavMeshHit navHit;
		NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);
		return navHit.position;
	}
}
