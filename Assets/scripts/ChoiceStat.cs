using System.Collections;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChoiceStat : MonoBehaviour {

	public TMP_Text title;
	public Image greenBar;
	public string optionA;
	public string optionB;
	public string playerPref;
	public string databaseID;

	private LanguageManager languageManager;
	private ChoiceMenu choiceMenu;
	int pref;
	private string res;

	void Start() {
		choiceMenu = GameObject.Find("Main Menu").GetComponent<ChoiceMenu>();
		languageManager = GameObject.Find("Main Menu").GetComponent<LanguageManager>();

		FetchResultWithoutDatabase();
		StartCoroutine(UpdateResult());
		StartCoroutine(Fetch());

	}

	// Will return a stat that does not include 'and x% of players'
	void FetchResultWithoutDatabase() {
		pref = PlayerPrefs.GetInt(playerPref);
		res = languageManager.GetString(optionB);
		if (pref == 0)
			res = languageManager.GetString(optionA);

		title.text = string.Format(res, "");
	}

	// Will send a GET request to the web server, fetching a JSON string of results
	IEnumerator Fetch() {
		choiceMenu.StartingLoad();

		using(UnityWebRequest webRequest = UnityWebRequest.Get(choiceMenu.choiceURL + "/" + databaseID)) {
			yield return webRequest.SendWebRequest();
			if (webRequest.isNetworkError) {
				Debug.Log("Error: " + webRequest.error);
			} else {
				JSONNode result = JSON.Parse(webRequest.downloadHandler.text);
				string newText = string.Format(res, languageManager.GetString("ui_choice_if_db"));
				float total = 100;
				if (result["total"] != 0) {
					// we can divide this number by zero
					if (pref == 0) // optionA
						total = (((float) result["votes_a"] / (float) result["total"]) * 100);
					else // optionB
						total = (((float) result["votes_b"] / (float) result["total"]) * 100);
				}
				greenBar.fillAmount = total / 100;
				title.text = string.Format(newText, Mathf.Round(total));
				choiceMenu.LoadingComplete();
			}
		}
	}

	// Will send a POST request to the web server, updating the correct stat by 1
	IEnumerator UpdateResult() {
		if (PlayerPrefs.GetInt("uploadResults ") == 1) {
			string choice = pref == 0 ? "a" : "b";
			WWWForm form = new WWWForm();
			form.AddField("choice", choice);

			using(UnityWebRequest www = UnityWebRequest.Post(choiceMenu.choiceURL + "/" + databaseID, form)) {
				yield return www.SendWebRequest();
				if (www.isNetworkError || www.isHttpError) Debug.Log(www.error);
			}
		}
	}
}