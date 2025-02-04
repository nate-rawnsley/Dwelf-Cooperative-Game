using UnityEngine;
using UnityEngine.Events;

//Finishes the level when both end doors are opened (function called by event).

public class GameManager : MonoBehaviour {
    private int doorsToOpen = 2;
    public UnityEvent OnWin;

    public void DoorChange(bool open) {
        if (open) {
            doorsToOpen--;
        } else {
            doorsToOpen++;
        }
        if (doorsToOpen == 0) {
            OnWin.Invoke(); 
        }
    }
}
