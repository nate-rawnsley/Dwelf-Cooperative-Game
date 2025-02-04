using UnityEngine;
using UnityEngine.Events;

//The script on switches, which trigger an event (affecting doors) when hit with the axe or arrows.
//Can be set to toggleable for levers, but this is unused within the game.

public class Button : MonoBehaviour {

    [SerializeField] private bool toggleable;
    public UnityEvent OnPress;
    public UnityEvent LeverReverse;
    private bool pressed;
    private Animator anim;
    private AudioSource audioSource;

    private void Awake() {
        anim = GetComponent<Animator>();
        audioSource = GameObject.Find("Canvas/Sound Effects").GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (pressed && !toggleable) {
            return;
        }
        audioSource.pitch = 0.8f;
        audioSource.PlayOneShot((AudioClip)Resources.Load("buttonClick"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
        if (toggleable) {
            pressed = !pressed;
            if (pressed) {
                OnPress.Invoke();
                anim.SetTrigger("Open");
            } else {
                LeverReverse.Invoke();
                anim.SetTrigger("Reverse");
            }
        } else {
            anim.SetTrigger("Open");
            OnPress.Invoke();
            pressed = true;
        }
    }
}
