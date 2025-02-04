using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

//Scrapped script for assigning player controllers by a menu.
//Replaced by Spawn Manager, which spawns when two controllers re connected.

public class ControllerManager : MonoBehaviour {
    private PlayerInputManager im;
    private static bool dwarfActive;
    private static bool elfActive;
    private static int dwarfDevice;
    private static int elfDevice;
    private List<GameObject> blankObjects = new List<GameObject>();
    private GameObject assignMenu;
    private CharacterSpawner dwarfSpawner;
    private CharacterSpawner elfSpawner;
    [SerializeField] private bool mainMenu;

    private void Awake() {
        im = GetComponent<PlayerInputManager>();
        assignMenu = GameObject.Find("Canvas/Controller Connect");
        if (mainMenu) {
            Debug.Log("Main menu detected.");
            return;
        }
        Debug.Log("Regular level detected.");
        dwarfSpawner = GameObject.Find("Spawn Points/Dwarf Spawn").GetComponent<CharacterSpawner>();
        elfSpawner = GameObject.Find("Spawn Points/Elf Spawn").GetComponent<CharacterSpawner>();
        Debug.Log($"Dwarf status: {dwarfActive}. Elf status: {elfActive}");
        if (dwarfActive && elfActive) {
            assignMenu.SetActive(false);
            SpawnCharacters();
        } else {
            DisplayStatus();
        }
    }

    private void OnPlayerJoined(PlayerInput input) {
        foreach (var device in input.devices) {
            Debug.Log(device.deviceId);
        }
        if (dwarfActive && elfActive) {
            Debug.Log("Player joined, but dwarf and elf are already there.");
            return;
        }
        blankObjects.Add(input.gameObject);
        //int index = input.playerIndex;
        Debug.Log($"Gamepads: {Gamepad.all.Count}");
        int index = int.MinValue;
        for (int i = 0; i < Gamepad.all.Count; i++) {
            if (Gamepad.all[i].deviceId == input.devices[0].deviceId) {
                index = i;
            }
        }
        if (!dwarfActive) {
            Debug.Log($"Dwarf joined: Index {index}, Gamepad {Gamepad.all[index]}");
            dwarfDevice = index;
            dwarfActive = true;
            //dwarfSpawner.SpawnCharacter(dwarfDevice, input.devices[0]);
        } else if (!elfActive) {
            Debug.Log($"Elf joined: Index {index}, Gamepad {Gamepad.all[index]}");
            elfDevice = index;
            elfActive = true;
            //elfSpawner.SpawnCharacter(elfDevice, input.devices[0]);
        }
        DisplayStatus();
    }

    private void DisplayStatus() {
        var dwarfText = assignMenu.transform.Find("Dwarf/Dwarf Status").GetComponent<TextMeshProUGUI>();
        var dwarfControlText = assignMenu.transform.Find("Dwarf/Dwarf Controller").GetComponent<TextMeshProUGUI>();
        if (dwarfActive) {
            dwarfText.text = "Connected!";
            dwarfControlText.text = Gamepad.all[dwarfDevice].ToString();
        } else {
            dwarfText.text = "Disconnected";
            dwarfControlText.text = "";
        }
        var elfText = assignMenu.transform.Find("Elf/Elf Status").GetComponent<TextMeshProUGUI>();
        var elfControlText = assignMenu.transform.Find("Elf/Elf Controller").GetComponent<TextMeshProUGUI>();
        if (elfActive) {
            elfText.text = "Connected!";
            elfControlText.text = Gamepad.all[elfDevice].ToString();
        } else {
            elfText.text = "Disconnected";
            elfControlText.text = "";
        }
        UnityEngine.UI.Button startButton = assignMenu.transform.Find("Buttons/Start Button").GetComponent<UnityEngine.UI.Button>();
        if (dwarfActive && elfActive) {
            startButton.interactable = true;
        } else {
            startButton.interactable = false;
        }
    }

    public void ResetCharacter() {
        Debug.Log("Resetting characters.");
        foreach (var obj in blankObjects) {
            if (obj != null) {
                Destroy(obj);
            }
        }
        blankObjects.Clear();
        dwarfSpawner.DestroyCharacter();
        elfSpawner.DestroyCharacter();
        dwarfActive = false;
        elfActive = false;
        DisplayStatus();
    }

    public void SpawnCharacters() {
        Debug.Log("Spawning characters.");
        //dwarfSpawner.SpawnCharacter(dwarfDevice);
        //elfSpawner.SpawnCharacter(elfDevice);
    }
}
