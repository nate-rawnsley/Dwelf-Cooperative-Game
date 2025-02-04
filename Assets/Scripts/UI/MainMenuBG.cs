using UnityEngine;

//The background for the main menu.
//Changes appearance if the player has won the game, or when going to level select.

public class MainMenuBG : MonoBehaviour {
    private Animator anim;
    private bool won;

    private void Awake() {
        anim = gameObject.GetComponent<Animator>();
        won = PlayerPrefs.GetInt("level", 1) > 12;
        if (won) {
            anim.SetTrigger("Win");
        }
    }

    public void Title() {
        if (won) {
            anim.SetTrigger("Win");
        } else {
            anim.SetTrigger("DefaultBG");
        }
    }

    public void LevelSelect() {
        if (!won) {
            anim.SetTrigger("Select");
        }
    }

    public void ResetProgress() {
        if (won) {
            anim.SetTrigger("Reset");
            won = false;
        }
    }
}
