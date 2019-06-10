using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainMenuUpdate : MonoBehaviour {

	public string webURL;
	public Button container;

	private MainMenu mainMenu;

	void Start() {
		mainMenu = GetComponent<MainMenu>();
		container.onClick.AddListener(VisitDownloadLink);

		UpdateUI();
	}

	// Updates the UI to show if an update is available or not
	void UpdateUI() {
		int state = PlayerPrefs.GetInt("updateAvailable");
		// 0 = unchecked, 1 = available, 2 = no updates

		container.gameObject.SetActive(false);
		if (state == 1)
			container.gameObject.SetActive(true);
		if (state == 0)
			StartCoroutine("Fetch");
	}

	// Will visit the download link if one exists when the button is clicked
	void VisitDownloadLink() {
		string url = PlayerPrefs.GetString("updateURL");
		if (url != "") {
			Application.OpenURL(url);
		}
	}

	// Reset what we know about whether there is an update
	void OnApplicationQuit() {
		PlayerPrefs.SetInt("updateAvailable", 0);
	}

	// Fetch the data
	IEnumerator Fetch() {
		using(UnityWebRequest webRequest = UnityWebRequest.Get(webURL)) {
			// Request and wait for the desired page.
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError) {
				Debug.Log("Error: " + webRequest.error);
			} else {
				JSONNode result = SimpleJSON.JSON.Parse(webRequest.downloadHandler.text);
				CheckVersion(result["version"], result["download_url"]);
			}
		}
	}

	// Compare the two versions found
	void CheckVersion(string version, string downloadURL) {
		PlayerPrefs.SetInt("updateAvailable", 2);

		if (version != mainMenu.version) {
			PlayerPrefs.SetInt("updateAvailable", 1);
			PlayerPrefs.SetString("updateURL", downloadURL);
			UpdateUI();
		}
	}
}