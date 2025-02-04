using UnityEngine;
using UnityEngine.InputSystem;

//Spawns the characters on scene load, attached to the correct controller.

public class CharacterSpawner : MonoBehaviour {
    [SerializeField] private GameObject charObject;
    public enum charType { Dwarf, Elf };
    [SerializeField] private charType character;
    private GameObject activePlayer;

    [Header("Toggle Dwarf abilities (if dwarf spawner)")]
    [SerializeField] private bool hasAxe = true;
    [Header("Toggle Elf abilities (if elf spawner)")]
    [SerializeField] private bool hasBow = true;

    public void SpawnCharacter() {
        if (activePlayer != null) {
            transform.position = activePlayer.transform.position;
            Destroy(activePlayer);
        }
        switch (character) {
            case charType.Dwarf:
                var gp1 = Gamepad.all[0];
                var a = PlayerInput.Instantiate(charObject, controlScheme: "Dwarf", pairWithDevice: gp1);
                activePlayer = a.gameObject;
                DwarfControls dc = activePlayer.GetComponent<DwarfControls>();
                dc.hasAxe = hasAxe;
                break;
            case charType.Elf:
                var gp0 = Gamepad.all[1];
                var b = PlayerInput.Instantiate(charObject, controlScheme: "Elf", pairWithDevice: gp0);
                activePlayer = b.gameObject;
                ElfControls ec = activePlayer.GetComponent<ElfControls>();
                ec.hasBow = hasBow;
                break;
        }
        activePlayer.transform.position = transform.position;
    }

    public void DestroyCharacter() {
        if (activePlayer != null) {
            transform.position = activePlayer.transform.position;
            Destroy(activePlayer);
            activePlayer = null;
        }
    }
}
