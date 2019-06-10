using UnityEngine;

public class EditionManager : MonoBehaviour {

    public void SetEdition(Edition edition) {
        PlayerPrefs.SetString("edition", edition.ToString());
    }

    public Edition GetEdition() {
        return (Edition) System.Enum.Parse(typeof(Edition), PlayerPrefs.GetString("edition").ToString());
    }

    public bool Is(Edition name) {
        return name == GetEdition();
    }
}
