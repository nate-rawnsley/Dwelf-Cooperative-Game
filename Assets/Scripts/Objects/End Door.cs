using UnityEngine;
using UnityEngine.Events;

//Controls the two end doors, which open when the correct player is in front.
//Sends a value to GameManager on enter and exit.

public class EndDoor : MonoBehaviour {
    public enum DoorType { Dwarf, Elf };
    [SerializeField] private DoorType doorType;
    private Animator animator;

    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    private void Awake() {
        animator = GetComponent<Animator>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if (doorType == DoorType.Dwarf && other.CompareTag("Dwarf") || doorType == DoorType.Elf && other.CompareTag("Elf")) {
            animator.SetBool("open", true);
            OnEnter.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (doorType == DoorType.Dwarf && other.CompareTag("Dwarf") || doorType == DoorType.Elf && other.CompareTag("Elf")) {
            animator.SetBool("open", false);
            OnExit.Invoke();
        }
    }
}
