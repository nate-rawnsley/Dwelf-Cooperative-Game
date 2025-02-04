using UnityEngine;

//The script on the axe projectile, spawned on awake and made active when the dwarf throws his axe.
//Becomes a platform when stuck in a wall, and can return to the dwarf (and have the return cancelled).

public class AxeProjectile : Projectile {
    public Vector3 rotation;
    private bool returning;
    private bool checking;
    private Transform dwarf;
    private BoxCollider2D handle;
    private int playerLayer;
    private Door doorScript;
    public bool interruptedReturn;
    private Vector2 returnDirection;

    protected override void Awake() {
        base.Awake();
        foreach(var collider in GetComponents<BoxCollider2D>()) {
            if (!collider.isTrigger) {
                handle = collider;
                break;
            }
        }
        playerLayer = 1 << LayerMask.NameToLayer("Player");
    }

    public override void StartFiring(Vector2 initialVelocity) {
        base.StartFiring(initialVelocity);
        handle.enabled = false;
        handle.isTrigger = true;
    }

    private void Update() {
        if (returning) {
            returnDirection = dwarf.position - transform.position;
            transform.up = -returnDirection;
            float forward = -15 * Time.deltaTime;
            transform.Translate(new Vector3(0, forward, 0));
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other) {
        base.OnTriggerEnter2D(other);
        if (!returning && moving && other.gameObject.layer != LayerMask.NameToLayer("Water")) {
            gameObject.layer = LayerMask.NameToLayer("Default");
            moving = false;
            rb.bodyType = RigidbodyType2D.Static;
            checking = true;
            handle.enabled = true;
            if (other.TryGetComponent<Door>(out Door door)) {
                doorScript = door;
            }
            audioSource.pitch = Random.Range(0.6f, 0.9f);
            audioSource.PlayOneShot((AudioClip)Resources.Load("projectileLand"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.CompareTag("Dwarf") && returning) {
            dwarf.gameObject.GetComponent<DwarfControls>().AxeReturned();
            gameObject.layer = LayerMask.NameToLayer("NoCollision");
            gameObject.SetActive(false);
            returning = false;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (interruptedReturn) {
            transform.up = -direction;
        }
        if (doorScript != null && doorScript.moving) {
            Vector3 pos = transform.position;
            float platformMomentum = (doorScript.endVal - doorScript.startVal) * doorScript.speed * Time.fixedDeltaTime;
            platformMomentum *= doorScript.open ? 1 : -1;
            if (doorScript.direction == Door.DoorDirection.left || doorScript.direction == Door.DoorDirection.right) {
                pos.x += platformMomentum;
            } else {
                pos.y += platformMomentum;
            }
            transform.position = pos;
        }
        if (checking) {
            RaycastHit2D playerInRadius = Physics2D.Raycast(transform.position, transform.up, -2.1f, playerLayer);
            Debug.DrawRay(transform.position, transform.up * -2.1f, Color.green);
            //Debug.Log(playerInRadius.collider.gameObject.name);
            if (playerInRadius.collider == null) {
                checking = false;
                handle.isTrigger = false;
            }
        }
    }

    public void SwitchBehaviour(Transform dwarfTransform) {
        if (interruptedReturn) {
            ResumeReturn();
        } else if (returning) {
            StopReturn();
        } else {
            ReturnToDwarf(dwarfTransform);
        }
    }

    public void ReturnToDwarf(Transform dwarfTransform) {
        gameObject.layer = LayerMask.NameToLayer("FlyingAxe");
        moving = false;
        checking = false;
        rb.bodyType = RigidbodyType2D.Static;
        dwarf = dwarfTransform;
        handle.isTrigger = true;
        doorScript = null;
        returning = true;
    }

    //When the dwarf is picked up or pressed return again, the axe falls back down.
    public void StopReturn() {
        if (returning) {
            returning = false;
            gameObject.layer = LayerMask.NameToLayer("NoCollision");
            interruptedReturn = true;
            StartFiring(returnDirection);
        }
    }

    public void ResumeReturn() {
        if (interruptedReturn) {
            ReturnToDwarf(dwarf);
            interruptedReturn = false;
        }
    }
}
