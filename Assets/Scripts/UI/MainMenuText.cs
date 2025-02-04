using UnityEngine;
using TMPro;

//Changes the text in the main menu to black if the background is post-win, for readability.
//Resets it to white when progress is reset.

public class MainMenuText : MonoBehaviour {
    private TextMeshProUGUI[] labels;

    private void Awake() {
        labels = GetComponentsInChildren<TextMeshProUGUI>();
        if (PlayerPrefs.GetInt("level", 1) > 12) {
            foreach (var label in labels) {
                label.color = Color.black;
            }
        }
    }

    public void ResetProgress() {
            foreach (var label in labels) {
            label.color = Color.white;
        }
    }
}
