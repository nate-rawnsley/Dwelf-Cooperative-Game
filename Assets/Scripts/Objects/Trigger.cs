using UnityEngine;
using UnityEngine.Events;

//An invisible trigger within the level, that activates a behaviour when entered.
//Used to make tutorials appear.

public class Trigger : MonoBehaviour {
    public UnityEvent OnEnter;

    private void OnTriggerEnter2D(Collider2D collision) {
        OnEnter.Invoke();
    }
}
