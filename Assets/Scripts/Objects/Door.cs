using UnityEngine;
using TMPro;

//The script on doors, with functions called by events from switches and buttons.
//Customiseable, allowing the designer to change how many inputs are needed, whether it's toggled or held and its behaviour.
//Lerps between the start and end positions, which are calucated on Start.

public class Door : MonoBehaviour {
    [SerializeField] private int inputsToOpen; //The number of inputs needed to open/close the door

    public enum DoorDirection { left, right, up, down };
    public DoorDirection direction;
    private enum DoorType { toggledOpen, heldOpen };
    [SerializeField] private DoorType type;

    [Range(0.5f, 10)][SerializeField] private float distanceMultiplier = 2;
    [Range(0.05f, 10)][SerializeField] public float speed = 0.2f;
    private int inputsReceived;

    private bool vertical;
    public bool open;
    public bool moving;
    public float startVal;
    public float endVal;
    private float time;

    private TextMeshProUGUI display;

    private void Start() {
        var sr = GetComponent<SpriteRenderer>();
        float height = sr.bounds.extents.y;
        float width = sr.bounds.extents.x;
        time = 0;
        switch (direction) { 
            case DoorDirection.left:
                vertical = false;
                startVal = transform.position.x;
                endVal = startVal - width * distanceMultiplier;
                break;
            case DoorDirection.right:
                vertical = false;
                startVal = transform.position.x;
                endVal = startVal + width * distanceMultiplier;
                break;
            case DoorDirection.up:
                vertical = true;
                startVal = transform.position.y;
                endVal = startVal + height * distanceMultiplier;
                break;
            case DoorDirection.down:
                vertical = true;
                startVal = transform.position.y;
                endVal = startVal - height * distanceMultiplier;
                break;
        }
    }

    private void DisplayNumber() {
        if (display != null) {
            display.text = (inputsToOpen - inputsReceived).ToString();
        }
    }

    public void Open(bool held) {
        if (type == DoorType.heldOpen) {
            if (held) {
                inputsReceived++;
            } else {
                inputsReceived--;
            }
            if (inputsReceived == inputsToOpen && !open) {
                open = true;
                moving = true;
            } else if (inputsReceived < inputsToOpen && open) {
                open = false;
                moving = true;
            }
            DisplayNumber();
        } else {
            inputsReceived++;
            DisplayNumber();
            if (inputsReceived == inputsToOpen) {
                open = !open;
                moving = true;
                inputsReceived = 0;
            }
        }
    }

    private void Update() { 
        if (moving) {
            Vector3 incrementPos = transform.position;
            if (vertical) {
                incrementPos.y = Mathf.Lerp(startVal, endVal, time);
            } else {
                incrementPos.x = Mathf.Lerp(startVal, endVal, time);
            }
            transform.position = incrementPos;
            time += open ? Time.deltaTime * speed : -Time.deltaTime * speed;
            if ((time >= 1 && open) || (time <= 0 && !open)) {
                moving = false;
            }
        }
    }
}
