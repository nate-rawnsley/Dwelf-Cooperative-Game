using UnityEngine;

//Controls whether level select buttons are active based on player progress.
//Checks on start, as well as when the player erases their progress.

public class LevelsParent : MonoBehaviour {
    LevelSelect[] buttons;

    private void Awake() {
        buttons = GetComponentsInChildren<LevelSelect>();
        CheckAllUnlocks();
    }

    public void CheckAllUnlocks() {
        foreach (var button in buttons) {
            button.CheckUnlock();
        }
    }
}
