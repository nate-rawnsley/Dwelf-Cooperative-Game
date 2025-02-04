using UnityEngine;
using UnityEngine.Events;

//Script on the 'Pressure Pad' object in levels, which activates the Door script when pressed and deactivates when released.

public class PressurePad : MonoBehaviour {
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    private SpriteRenderer sRenderer;
    public UnityEvent OnPress;
    public UnityEvent OnRelease;
    private AudioSource audioSource;

    private int onButton = 0;

    private void Awake() {
        sRenderer = GetComponent<SpriteRenderer>();
        audioSource = GameObject.Find("Canvas/Sound Effects").GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Dwarf") || other.CompareTag("Elf")) {
            if (onButton == 0) {
                sRenderer.sprite = offSprite;
                OnPress.Invoke();
            }
            onButton++;
            audioSource.pitch = 0.95f;
            audioSource.PlayOneShot((AudioClip)Resources.Load("buttonClick"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Dwarf") || other.CompareTag("Elf")) {
            if (onButton == 1) {
                sRenderer.sprite = onSprite;
                OnRelease.Invoke();
            }
            onButton--;
            audioSource.pitch = 0.7f;
            audioSource.PlayOneShot((AudioClip)Resources.Load("buttonClick"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
        }
    }
}
