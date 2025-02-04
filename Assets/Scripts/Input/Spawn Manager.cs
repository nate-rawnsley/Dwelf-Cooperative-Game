using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//Spawns the players at their spawners when the game starts, with their proper controllers.
//Uses the first two controllers connected to the computer, waits if there aren't 2.

public class SpawnManager : MonoBehaviour {
    private CharacterSpawner[] spawners;
    private GameObject controllerPrompt;

    private void Awake() {
        spawners = GetComponentsInChildren<CharacterSpawner>();
        controllerPrompt = GameObject.Find("Canvas/Controller Prompt");
        if (Gamepad.all.Count < 2) {
            StartCoroutine(WaitForControllers());
        } else {
            PromptSpawners();
        }
    }

    private IEnumerator WaitForControllers() {
        bool controllersFound = false;
        while (!controllersFound) {
            controllersFound = Gamepad.all.Count >= 2;
            yield return new WaitForSeconds(2);
        }
        PromptSpawners();
    }

    private void PromptSpawners() {
        controllerPrompt.SetActive(false);
        foreach (var spawner in spawners) {
            spawner.SpawnCharacter();
        }
    }
}
