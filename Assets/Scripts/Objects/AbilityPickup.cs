using System.Collections;
using UnityEngine;
using UnityEngine.Events;

//The script on the Bow and Axe pickups in levels 5 and 10.
//Uses a lerp to stand out in scene and plays custom music and animation when picked up.

public class AbilityPickup : MonoBehaviour {
    public enum Type { Axe, Bow };
    public Type type;

    [SerializeField] private GameObject glow;

    public UnityEvent OnPickup;

    private float topPos;
    private float bottomPos;
    private float time;
    private bool onFloor = true;

    private void Start() {
        float pos = transform.position.y;
        topPos = pos + 0.5f;
        bottomPos = pos - 0.5f;
        time = 0.5f;
    }

    private void Update() {
        if (!onFloor) {
            return;
        }
        float perc = Easing.Sine.InOut(time);
        float stepPos = Mathf.Lerp(topPos, bottomPos, perc);
        transform.position = new Vector2(transform.position.x, stepPos);
        time += Time.deltaTime * 0.4f;
        time %= 2;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        string tag = other.tag;
        switch (type) {
            case Type.Axe:
                if (tag == "Dwarf") {
                    other.gameObject.SendMessage("GetAxe");
                    var tempPos = other.transform.position;
                    tempPos.y += 2.5f;
                    StartCoroutine(PickUp(tempPos));
                }
                break;
            case Type.Bow:
                if (tag == "Elf") {
                    other.gameObject.SendMessage("GetBow");
                    var tempPos = other.transform.position;
                    tempPos.y += 4;
                    StartCoroutine(PickUp(tempPos));
                }
                break;
        }
    }

    private IEnumerator PickUp(Vector2 pos) {
        onFloor = false;
        transform.position = pos;
        Vector3 glowPos = pos;
        glowPos.z = 0.1f;
        GameObject glowObj = Instantiate(glow, glowPos, Quaternion.identity);
        AudioSource musicSource = GameObject.Find("Canvas/Music").GetComponent<AudioSource>();
        AudioClip oldMusic = musicSource.clip;
        float oldTime = musicSource.time;
        float oldVolume = musicSource.volume;
        //music fadeout
        for (float t = 0; t < 1; t += Time.deltaTime) {
            musicSource.volume = oldVolume - (oldVolume * t);
            yield return null;
        }
        musicSource.volume = PlayerPrefs.GetFloat("musicVol", 0.5f) * 0.5f; 
        musicSource.clip = (AudioClip)Resources.Load("pickupJingle");
        musicSource.Play();
        yield return new WaitForSeconds(4);
        Destroy(glowObj);
        OnPickup.Invoke();
        musicSource.clip = oldMusic;
        musicSource.time = oldTime;
        musicSource.volume = 0;
        musicSource.Play();
        //music fade in
        for (float t = 0; t < 1; t += Time.deltaTime) {
            musicSource.volume = oldVolume * t;
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
