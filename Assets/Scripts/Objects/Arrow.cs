using UnityEngine;

//The script on arrows, adding some specific behaviour and unique sound effect pitch.

public class Arrow : Projectile {

    protected override void OnTriggerEnter2D(Collider2D other) {
        base.OnTriggerEnter2D(other);
        if (other.gameObject.layer != LayerMask.NameToLayer("Water")) {
            moving = false;
            rb.bodyType = RigidbodyType2D.Static;
            audioSource.pitch = Random.Range(1.3f, 1.6f);
            audioSource.PlayOneShot((AudioClip)Resources.Load("projectileLand"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
        }
    }
}
