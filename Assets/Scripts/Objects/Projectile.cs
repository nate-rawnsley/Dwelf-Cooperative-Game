using UnityEngine;

//The parent class of axe and arrow projectiles. handling movement through the air using physics.

public class Projectile : MonoBehaviour {
    protected Vector2 direction;
    protected bool moving;
    protected Rigidbody2D rb;
    protected AudioSource audioSource;

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody2D>();
        audioSource = audioSource = GameObject.Find("Canvas/Sound Effects").GetComponent<AudioSource>();
        gameObject.SetActive(false);
    }

    public virtual void StartFiring(Vector2 initialVelocity) {
        gameObject.SetActive(true);
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = initialVelocity;
        moving = true;
    }

    protected virtual void FixedUpdate() {
        if (!moving) {
            return;
        }
        direction = rb.velocity.normalized;
        transform.up = direction;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water")) {
            rb.drag = 0.5f;
            rb.gravityScale = 0.2f;
            audioSource.clip = (AudioClip)Resources.Load("projectileSplash");
            audioSource.volume = PlayerPrefs.GetFloat("sfxVol", 0.5f);
            audioSource.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water")) {
            rb.drag = 0;
            rb.gravityScale = 1;
        }
    }
}
