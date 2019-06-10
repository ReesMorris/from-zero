using UnityEngine;
using UnityEngine.UI;

public class PauseMenuChoice : MonoBehaviour {

	public enum Identifiers {resume, settings, mainMenu, quit};
	public Identifiers identifier;
	public string titleKey;
	public string dialogueKey;
	public string acceptButtonKey;
	public string rejectButtonKey;

	private PauseManager pauseManager;
    private EditionManager editionManager;

	void Start() {
		GetComponent<Button>().onClick.AddListener(OnClick);
        pauseManager = GameObject.Find ("GameManager").GetComponent<PauseManager> ();
        editionManager = GameObject.Find("GameManager").GetComponent<EditionManager>();

        CheckEdition();
	}

	void OnClick() {
		pauseManager.OnMenuItemClick (this);
	}

    void CheckEdition() {
        if(editionManager.Is(Edition.Kongregate) && identifier == Identifiers.quit) {
            gameObject.SetActive(false);
        }
    }
}
