using UnityEngine;

//Ensures music in all levels has its volume changed to match the value set by the player.

public class MusicManager : MonoBehaviour {
    public float multiplier = 1;
    private AudioSource source;

    private void Awake() {
        source = GetComponent<AudioSource>();
        source.volume = PlayerPrefs.GetFloat("musicVol", 0.5f) * multiplier;
    }

    //Called from UI slider
    public void ChangeVolume(float value) {
        source.volume = value * multiplier;
    }
}
