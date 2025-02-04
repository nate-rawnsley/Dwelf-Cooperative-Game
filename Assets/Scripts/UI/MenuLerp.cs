using System.Collections;
using UnityEngine;
using UnityEngine.Events;

//Reuseable script to add lerping to a UI element.
//Fully customisable and modular, designed to be applicable to any UI.

public class MenuLerp : MonoBehaviour {
    [SerializeField] private Vector2 offset;
    [SerializeField] private Easing.LerpType type;
    [SerializeField] private Easing.LerpMotion motion;
    [SerializeField] private bool reverseMotion; //eg. use In forward and Out backward.
    [SerializeField] private float forwardDelay;
    [SerializeField] private float reverseDelay;
    [SerializeField] private float speed = 1;
    [SerializeField] private bool moveOnStart;
    [SerializeField] private bool startOffscreen; //set current position to end, and reposition to the start on Awake.
    [SerializeField] private bool useTime = true; //use a fixed value instead of deltaTime, if run while game is paused.
    public UnityEvent OnForwardFinish;
    public UnityEvent OnReverseFinish;
    private Easing.LerpMotion useMotion;
    private Vector2 initialPos;
    private Vector2 endPos;
    private bool moving;

    private void Awake() {
        if (startOffscreen) {
            endPos = transform.position;
            initialPos = endPos - offset;
            transform.position = initialPos;
        } else {
            initialPos = transform.position;
            endPos = initialPos + offset;
        }
        if (moveOnStart) {
            StartMove();
        }
    }

    public void StartMove() {
        if (moving) {
            return;
        }
        useMotion = motion;
        if (forwardDelay > 0) {
            StartCoroutine(DelayStart(forwardDelay, true));
        } else {
            StartCoroutine(LerpBehaviour(true));
        }
    }

    public void ReverseMove() {
        if (moving) {
            return;
        }
        if (reverseMotion) {
            useMotion = motion == Easing.LerpMotion.In ? Easing.LerpMotion.Out : motion == Easing.LerpMotion.Out ? Easing.LerpMotion.In : motion;
        }
        if (reverseDelay > 0) {
            StartCoroutine(DelayStart(reverseDelay, false));
        } else {
            StartCoroutine(LerpBehaviour(false));
        }
    }

    private IEnumerator DelayStart(float delay, bool dir) {
        yield return new WaitForSeconds(delay);
        StartCoroutine(LerpBehaviour(dir));
    }

    private IEnumerator LerpBehaviour(bool dir) {
        moving = true;
        float t = 0;
        Vector2 start;
        Vector2 end;
        if (dir) {
            start = initialPos; 
            end = endPos;
        } else {
            start = endPos;
            end = initialPos;
        }
        while (t < 1) {
            float perc = Easing.LerpSwitch(t, type, motion);
            Vector2 pos = Vector2.Lerp(start, end, perc);
            transform.position = pos;
            t += useTime ? Time.deltaTime * speed : 0.02f * speed;
            yield return null;
        }
        moving = false;
        if (dir) {
            OnForwardFinish.Invoke();
        } else {
            OnReverseFinish.Invoke();
        }
    }
}
