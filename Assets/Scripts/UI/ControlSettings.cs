using UnityEngine;

//Controls despawning players on death and resuming when closing pause menu.
//Formerly handled switching controls between players, but this was scrapped.

public class ControlSettings : MonoBehaviour {
    private UI uiScript;
    public bool paused;

    private void Start() {
        uiScript = GetComponentInParent<UI>();
    }

    public void PlayerDeath() {
        GameObject.Find("Elf(Clone)").SetActive(false);
        GameObject.Find("Dwarf(Clone)").SetActive(false);
        GameObject throwingAxe = GameObject.Find("Axe Projectile(Clone)");
        if (throwingAxe != null) {
            throwingAxe.GetComponent<AxeProjectile>().StopReturn();
        }
        uiScript.DeathMenu();
    }

    public void Resume() {
        Time.timeScale = 1;
    }
}
