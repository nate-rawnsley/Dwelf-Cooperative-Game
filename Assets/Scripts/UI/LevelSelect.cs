using UnityEngine;
using UnityEngine.UI;

//The script on the level select buttons, making them uninteractable if the player hasn't unlocked their level yet.

public class LevelSelect : MonoBehaviour {
    [SerializeField] private int level;

    public void CheckUnlock() {
        if (PlayerPrefs.GetInt("level", 1) < level) {
            GetComponent<UnityEngine.UI.Button>().interactable = false;
        }
    }
}
