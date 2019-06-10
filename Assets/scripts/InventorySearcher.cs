using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventorySearcher : MonoBehaviour {

	public bool allowSmartSearch = true;
	public int currentObject = 0;
	public GameObject pickupTextObject;
	public TextMeshProUGUI pickupText;
	private Inventory inventory;
	private InputManager inputManager;
	private CutsceneManager cutsceneManager;
	private SoundManager soundManager;
	private ConsoleManager consoleManager;
	private StoryManager storyManager;
	private UIManager uiManager;
	private Entity entity;
	private float nextScrollTime;
	private bool smartSearch = true;
	public bool SmartSearch {
		get {
			return smartSearch;
		}
		set {
			smartSearch = value;
		}
	}

	void Start() {
		nextScrollTime = 0;
		storyManager = GameObject.Find ("GameManager").GetComponent<StoryManager> ();
		inputManager = GameObject.Find ("GameManager").GetComponent<InputManager> ();
		cutsceneManager = GameObject.Find ("GameManager").GetComponent<CutsceneManager> ();
		consoleManager = GameObject.Find ("GameManager").GetComponent<ConsoleManager> ();
		uiManager = GameObject.Find ("GameManager").GetComponent<UIManager> ();
		soundManager = GameObject.Find ("GameManager").GetComponent<SoundManager> ();
		entity = GetComponent<Entity> ();
		inventory = GetComponent<Inventory> ();
	}

	void Update () {
		SearchNearbyPickups ();
		CycleInventory ();
		ScrollInventory ();
	}

	void SearchNearbyPickups() {
		if (!cutsceneManager.CutsceneActive && !consoleManager.ConsoleOpen) {
			Collider[] hitColliders = Physics.OverlapSphere (transform.position, 2f, LayerMask.GetMask ("Pickup"));

			if (hitColliders.Length > 0) {
				// something has gone wrong and our array isn't big enough.. let's fix that
				if (currentObject > hitColliders.Length - 1) {
					currentObject = 0;
				}

				// Smart Searching will show the item needed most to the user
				// For example, if the player is on low health then health kits will be the first thing prioritised
				// Smart searching can be stopped by cycling between other items, and will restart when the player moves
				if (smartSearch && allowSmartSearch) {
					bool smartSearchFound = false;
					if (hitColliders.Length > 1) {
						
						// If the player is on 1/2 of their health or less, let's prioritise showing health kits
						if (entity.CurrentHealth <= (float)(entity.maxHealth / 2)) {
							for (int i = 0; i < hitColliders.Length; i++) {
								Pickup pickup = hitColliders [i].GetComponent<Pickup> ();

								if (pickup != null) {
									// If this item is rarer, prioritise showing it to the player
									if (pickup.pickupType == Pickups.Type.Health) {
										smartSearchFound = true;
										currentObject = i;
										break;
									}
								}
							}
						} 

						// All other searches failed, so let's give the item with the best rarity
						if (!smartSearchFound) {
							int bestRarity = 0;
							for (int i = 0; i < hitColliders.Length; i++) {
								Pickup pickup = hitColliders [i].GetComponent<Pickup> ();

								if (pickup != null) {
									// If this item is rarer, prioritise showing it to the player
									if (pickup.rarity > bestRarity) {
										bestRarity = pickup.rarity;
										currentObject = i;
									}
								}
							}
						}
					}
				}

				// Show the pickup text near the weapon
				if (!cutsceneManager.CutsceneActive) {
					pickupTextObject.SetActive (true);
					pickupTextObject.transform.position = hitColliders [currentObject].transform.position;
					pickupText.text = "[" + inputManager.GetKeyValue ("INVENTORY_PICK") + "] " + hitColliders [currentObject].gameObject.name;
				} else {
					pickupTextObject.SetActive (false);
				}

				// User presses pickup key
				if (inputManager.InputMatchesKeyup ("INVENTORY_PICK")) {

					// Add to inventory, if it's not a readable
					Readable readable = hitColliders[currentObject].GetComponent<Readable>();
					Listenable listenable = hitColliders[currentObject].GetComponent<Listenable>();
					Teleportable teleportable = hitColliders[currentObject].GetComponent<Teleportable>();
					Interactable interactable = hitColliders[currentObject].GetComponent<Interactable>();
					StoryTriggerable triggerable = hitColliders [currentObject].GetComponent<StoryTriggerable> ();
					if (readable != null) {
						if (readable.sprite != null && !uiManager.ReadableShowing) {
							uiManager.ShowReadable (readable);
						}
					} else if (listenable != null) {
						if (listenable.audioSource != null) {
							if (listenable.useCustomKeys) {
								soundManager.ToggleSound (listenable.audioSource);
							} else {
								soundManager.PlaySound (listenable.audioSource, 0f);
							}
							if (listenable.destroyOnActivate) {
								Destroy (listenable.gameObject);
							}
						}
					} else if (teleportable != null) {
						if (teleportable.destination != null) {
							storyManager.TeleportEntity (storyManager.player, teleportable.destination.transform.position);
						}
					} else if (interactable != null) {
						interactable.OnInteraction ();
					} else if (triggerable != null) {
						triggerable.OnTrigger ();
					} else {
						inventory.AddItemToInventory (hitColliders [currentObject].gameObject);
						currentObject = 0;
					}
				}

				// User presses cycle key
				if (inputManager.InputMatchesKeyup ("INVENTORY_CYCLE")) {
					smartSearch = false;
					currentObject = (currentObject + 1) % hitColliders.Length;
				}
			} else {
				pickupTextObject.SetActive (false);
				currentObject = 0;
			}
		}
	}

	void CycleInventory() {
		if (!cutsceneManager.CutsceneActive && !consoleManager.ConsoleOpen) {
			if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_1")) {
				inventory.EquipInventorySlot (0);
			} else if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_2")) {
				inventory.EquipInventorySlot (1);
			} else if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_3")) {
				inventory.EquipInventorySlot (2);
			} else if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_4")) {
				inventory.EquipInventorySlot (3);
			} else if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_5")) {
				inventory.EquipInventorySlot (4);
			} else if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_6")) {
				inventory.EquipInventorySlot (5);
			} else if (inputManager.InputMatchesKeyup ("INVENTORY_SLOT_7")) {
				inventory.EquipInventorySlot (6);
			}
		}
	}

	// Function to scroll through inventory slots
	void ScrollInventory() {
		if (!cutsceneManager.CutsceneActive && !consoleManager.ConsoleOpen) {
			// Especially on a trackpad, it can go too quickly. This will make it slower to scroll and easier for the player
			if (Time.time > nextScrollTime) {
				// Scrolling up
				if (Input.GetAxis ("Mouse ScrollWheel") > 0f) {
					inventory.ScrollToNextItem (0);
					nextScrollTime = Time.time + 0.3f;
				}
				// Scrolling down
				if (Input.GetAxis ("Mouse ScrollWheel") < 0f) {
					inventory.ScrollToNextItem (1);
					nextScrollTime = Time.time + 0.3f;
				}
			}
		}
	}
}
