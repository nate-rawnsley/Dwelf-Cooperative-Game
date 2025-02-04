using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static PhysicsExtensionFunctions;

//Floor detection and movement were taken from Catlike Coding https://catlikecoding.com/unity/tutorials/movement/physics/
//The main controller for characters, inherited from by DwarfControls and ElfControls.
//Handles all basic behaviour including walking, jumping, climbing ladders, falling, playing sounds and drawing trajectory.
//Specific values are overwritten in child classes, but creates the basic behaviour of a character.

public class PlayerController : MonoBehaviour {

    [Header("Attributes (read only)")]
    private Transform canvas;
    protected GameObject currentLine;
    protected LineRenderer lineRenderer;
    protected InputAction aimPos;
    protected bool aiming;
    protected float lineLength;

    private Transform pauseOverlay;
    private MenuLerp pauseLerpScript;

    protected Vector3 aimingVector;
    protected float projectileAcceleration;

    public enum State { Grounded, Jumping, Falling, Climbing};
    public State curState = State.Falling;

    private InputAction move;
    private bool moving;
    protected float velocityX;

    protected float speedModifier = 0;
    protected float jumpSpeed = 0;
    protected float maxFallSpeed = -2;
    protected float jumpStartSpeed = 0;
    public float playerHeight;
    protected float playerWidth;
    protected float jumpVal;
    private float coyoteTimeEnd;
    private float jumpBufferEnd;

    protected AudioSource audioSource;
    protected float actionPitch;
    private bool landPlaying;
    private bool walkPlaying;
    private bool ladderPlaying;

    private float ladderX;
    private bool climbing;
    private bool ladderLerping;
    private InputAction climb;
    private float ladderCooldownEnd;

    public bool active = true;
    public bool moveActive = true;

    protected int defaultLayerMask;

    protected bool wallLeft;
    protected bool wallRight;
    private bool waterEnter;
    private bool inWater;

    private Door platformBelow;

    protected PlayerInput input;
    protected Animator animator;
    protected ControlSettings pauseControls;
    protected Rigidbody2D rb;
    private SpriteRenderer sr;
    private SpriteRenderer ladderRenderer;

    protected virtual void Awake() {
        canvas = GameObject.Find("Canvas").transform;
        input = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        pauseOverlay = canvas.Find("Pause Overlay");
        GameObject pauseMenu = canvas.Find("Pause Menu").gameObject;
        pauseLerpScript = pauseMenu.GetComponents<MenuLerp>()[0];
        pauseControls = pauseMenu.GetComponent<ControlSettings>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        defaultLayerMask = 1 << LayerMask.NameToLayer("Default");
        playerHeight = sr.bounds.extents.y;
        playerWidth = sr.bounds.extents.x;
        currentLine = (GameObject)Instantiate(Resources.Load("Line"));
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        move = input.actions.FindAction("Move");
        audioSource = GetComponent<AudioSource>();
    }

    protected virtual void PlaySound(string soundName, float pitch, float randomRange = 0, bool loop = false, float volumeMod = 1) {
        if (!loop) {
            walkPlaying = false;
            ladderPlaying = false;
        }
        pitch = Random.Range(pitch - randomRange, pitch + randomRange);
        audioSource.pitch = pitch;
        AudioClip sound = (AudioClip)Resources.Load(soundName);
        audioSource.loop = loop;
        audioSource.clip = sound;
        audioSource.volume = PlayerPrefs.GetFloat("sfxVol", 0.5f) * volumeMod;
        audioSource.volume *= inWater ? 0.25f : 1;
        audioSource.Play();
    }

    protected void Animate(string name, float value = float.NaN) {
        if (Time.timeScale < 1 || !active) {
            return;
        }
        if (float.IsNaN(value)) {
            animator.SetTrigger(name);
        } else {
            animator.SetFloat(name, value);
        }
    }

    protected virtual void OnJump() {
        if (curState == State.Climbing) { //If on ladder, dismount.
            animator.SetBool("climb", false);
            Animate("idle");
            rb.bodyType = RigidbodyType2D.Dynamic;
            ladderCooldownEnd = Time.time + 0.5f;
            if (climb.ReadValue<float>() < 0) { //If climbing down, drop instead of jumping
                Animate("fall");
                curState = State.Falling;
                jumpVal = 0;
                return;
            }
        } 
        
        else if (curState != State.Grounded && coyoteTimeEnd < Time.time) {
            jumpBufferEnd = Time.time + 0.25f;
            return;
        }

        platformBelow = null;
        coyoteTimeEnd = Time.time;
        jumpBufferEnd = Time.time;
        curState = State.Jumping;
        jumpVal = jumpStartSpeed;
        Animate("jump");
        PlaySound("jump", actionPitch, 0.2f);
    }

    protected virtual void OnJumpEnd() {
        if (curState != State.Jumping) {
            return;
        }
        jumpBufferEnd = Time.time;
        jumpVal *= 0.75f;
    }

    protected virtual void OnMove(InputValue value) {
        if (value.Get<float>() == 0) {
            moving = false;
            velocityX = 0;
            if (curState == State.Grounded) {
                Animate("idle");
            }
        } else {
            moving = true;
        }
    }

    protected virtual void OnClimb(InputValue value) {
        if (value.Get<float>() == 0) {
            climbing = false;
            return;
        } else {
            climb = input.actions.FindAction("Climb");
            climbing = true;
        }
    }

    protected virtual void OnPause() {
        if (Time.timeScale > 0) {
            pauseOverlay.gameObject.SetActive(true);
            pauseLerpScript.StartMove();
            audioSource.Stop();
            walkPlaying = false;
            ladderPlaying = false;
            Time.timeScale = 0;
        } else {
            pauseControls.Resume();
        }
    }

    protected bool RayCheck(Vector3 checkPos, Vector3 checkDirection, int layerMask) {
        RaycastHit2D[] hits = Physics2D.RaycastAll(checkPos, checkDirection, 0.05f, layerMask);
        Debug.DrawRay(checkPos, checkDirection * 0.05f, Color.green);
        bool rayHit = false;
        foreach (var hit in hits) {
            var tag = hit.collider.tag;
            if (hit.collider != null) {
                Door dScript = hit.transform.gameObject.GetComponent<Door>();
                if (dScript != null && (dScript.direction == Door.DoorDirection.left || dScript.direction == Door.DoorDirection.right)) {
                    platformBelow = dScript;
                } else {
                    platformBelow = null;
                }
                rayHit = true;
            }
        }
        if (!rayHit) {
            platformBelow = null;
        }
        return rayHit;
    }

    private bool CheckIfGrounded() {
        //Sends out three small raycasts (left, centre, and right of player, downwards) to check if the object is grounded.
        bool grounded = false;
        bool newGround = false;

        for (int i = 0; i < 3; i++) {
            Vector3 floorCheckPos = transform.position;
            floorCheckPos.x -= playerWidth * (i - 1) * 0.5f;
            floorCheckPos.y -= playerHeight;
            if (RayCheck(floorCheckPos, Vector3.down, defaultLayerMask)) {
                grounded = true;
            }
            if (grounded) {
                animator.ResetTrigger("fall");
                if (curState != State.Grounded && moveActive) {
                    newGround = true;
                }
                curState = State.Grounded;
                if (jumpBufferEnd > Time.time && curState != State.Jumping) {
                    curState = State.Jumping;
                    jumpVal = jumpStartSpeed;
                    jumpBufferEnd = Time.time;
                    coyoteTimeEnd = Time.time;
                    Animate("jump");
                    return false;
                }
            } else if (curState == State.Grounded) {
                coyoteTimeEnd = Time.time + 0.125f;
                curState = State.Falling;
                jumpVal = 0;
                platformBelow = null;
                Animate("fall");
            }
        }
        return newGround;
    }

    //public virtual void ChangeControls(ControlSettings.ControlScheme scheme) { }

    //Checks if the player is touching the ceiling or walls.
    private void CheckCollision(Collision2D collision) {
        for (int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y <= -0.9f) {
                jumpVal *= 0.9f;
            } else if (normal.x <= -0.9f) { //this is 'else' because else it can be wrongly called if the player clips into the ceiling
                wallRight = true;
            } else if (normal.x >= 0.9f) {
                wallLeft = true;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        CheckCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        CheckCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water")) {
            if (curState == State.Grounded) {
                waterEnter = true;
            } else {
                WaterEnterBehaviour();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water")) {
            waterEnter = false;
            WaterExitBehaviour();
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Lava")) {
            Vector3 pos = transform.position;
            pos.y -= playerHeight * 0.75f;
            if (RayCheck(pos, Vector3.up, 1 << LayerMask.NameToLayer("Lava"))) {
                GameObject burnSoundEmitter = (GameObject)Instantiate(Resources.Load("LavaSound"));
                burnSoundEmitter.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("sfxVol", 0.5f);
                OnDeath();
            }
        }
    }

    protected virtual void WaterEnterBehaviour() {
        waterEnter = false;
        inWater = true;
        maxFallSpeed = -0.05f;
        rb.drag = 0.75f;
        rb.angularDrag = 0.75f;
        rb.gravityScale = 0.05f;
        audioSource.pitch = actionPitch + Random.Range(-0.05f, 0.05f);
        audioSource.PlayOneShot((AudioClip)Resources.Load("waterEnter"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
    }

    protected virtual void WaterExitBehaviour() {
        inWater = false;
        maxFallSpeed = -2;
        rb.drag = 0;
        rb.gravityScale = 1;
        audioSource.pitch = actionPitch + Random.Range(-0.05f, 0.05f);
        audioSource.PlayOneShot((AudioClip)Resources.Load("waterExit"), PlayerPrefs.GetFloat("sfxVol", 0.5f));
    }

    private void CheckForLadder() {
        if (ladderCooldownEnd > Time.time) {
            return;
        }
        int interactMask = LayerMask.GetMask("Interactable");
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, 0.3f, interactMask);
        foreach (var hitObject in hitObjects) {
            if (hitObject.CompareTag("Ladder")) {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                curState = State.Climbing;
                ladderRenderer = hitObject.GetComponent<SpriteRenderer>();
                jumpVal = 0;
                platformBelow = null;

                StartCoroutine(LadderMove(transform.position.x));
                animator.SetBool("climb", true);
                Animate("idle");
                break;
            }
        }
    }

    private float LadderClimb() {
        float ladderHeight = ladderRenderer.bounds.extents.y;
        float ladderCentre = ladderRenderer.bounds.center.y;
        
        float climbHeight = sr.bounds.extents.y;

        Vector2 ladderBounds = new Vector2(ladderCentre - ladderHeight, ladderCentre + ladderHeight);
        Vector2 topCheck = new Vector2(ladderX, ladderBounds.y);
        RaycastHit2D upHit = Physics2D.Raycast(topCheck, Vector2.up, climbHeight, defaultLayerMask);
        ladderBounds.y += upHit.collider != null ? upHit.distance - climbHeight : 0;
        Vector2 bottomCheck = new Vector2(ladderX, ladderBounds.x);
        RaycastHit2D downHit = Physics2D.Raycast(bottomCheck, Vector3.down, climbHeight, defaultLayerMask);
        ladderBounds.x -= downHit.collider != null ? downHit.distance - climbHeight : 0;

        float max = ladderBounds.y - transform.position.y;
        float min = ladderBounds.x - transform.position.y;
        float movement = climb.ReadValue<float>() * Time.fixedDeltaTime * 5;
        movement = Mathf.Clamp(movement, min, max);
        Animate("climbDir", climb.ReadValue<float>());
        return movement;
    }

    private IEnumerator LadderMove(float startX) {
        ladderLerping = true;
        for (float i = 0; i < 1; i += Time.deltaTime * 5) {
            float fixedX = Mathf.Lerp(startX, ladderX, i);
            Vector3 fixedPosition = new Vector3(fixedX, transform.position.y, transform.position.z);
            transform.position = fixedPosition;
            yield return null;
        }
        ladderLerping = false;
    }

    protected void OnDeath() {
        pauseControls.PlayerDeath();
        pauseOverlay.gameObject.SetActive(true);
    }

    protected void DrawTrajectory(bool type, float modifier, float lineMax) {
        int LINE_POINTS = 15;
        float angle = (Mathf.Atan2(aimingVector.y, aimingVector.x) * 180 / Mathf.PI + 270) % 360;
        if (type) {
            animator.SetBool("aimDir", angle > 180);
            float aimAngle = angle > 180 ? 180 - (360 - angle) : 180 - angle;
            Animate("aimAngle", aimAngle);
        }
        Vector3[] linePoints = new Vector3[LINE_POINTS];
        linePoints[0] = new Vector2(transform.position.x, transform.position.y + 0.5f); ;
        Vector3 interceptPos = Vector3.zero;
        for (float i = 1; i < LINE_POINTS; i++) {
            Vector3 incrementPos = linePoints[0];
            if (interceptPos != Vector3.zero) {
                incrementPos = interceptPos;
            }
            else {
                float xMod = inWater ? 0.5f : 0;
                float yMod = inWater ? 0.2f : 1;
                incrementPos.x += QuadraticDisplacement(lineLength * aimingVector.x * modifier, xMod, i / LINE_POINTS);
                incrementPos.y += QuadraticDisplacement(lineLength * aimingVector.y * modifier, -Physics2D.gravity.y * yMod, i / LINE_POINTS);
                Vector3 direction = incrementPos - linePoints[(int)i - 1];
                RaycastHit2D[] hits = Physics2D.RaycastAll(linePoints[(int)i - 1], direction.normalized, direction.magnitude, defaultLayerMask);
                foreach (var hit in hits) {
                    interceptPos = hit.point;
                    incrementPos = interceptPos;
                }
                
            }
            linePoints[(int)i] = incrementPos;
        }
        lineRenderer.positionCount = LINE_POINTS;
        lineRenderer.SetPositions(linePoints);
        if (lineLength < lineMax) {
            projectileAcceleration -= Time.fixedDeltaTime / 4.75f;
            lineLength += projectileAcceleration;
        }
    }

    protected virtual void FixedUpdate() {
        if (Time.timeScale < 1 || !active) { //Don't run behaviour when the game is paused or player inactive.
            return;
        }
        Vector3 frameMovement = new Vector3();
        if (climbing) {
            if (curState != State.Climbing) {
                CheckForLadder();
            }
        } 
        if (curState == State.Climbing) {
            ladderX = ladderRenderer.bounds.center.x;
            if (!ladderLerping) {
                frameMovement.x = ladderX - transform.position.x;
            }
            frameMovement.y += LadderClimb();
        }
        if (curState == State.Jumping) {
            if (jumpVal > 0) {
                jumpVal -= Time.fixedDeltaTime / jumpSpeed * 15;
            }
            else {
                curState = State.Falling;
            }
            frameMovement.y += jumpVal / 2;
        }
        else if (curState == State.Falling && jumpVal < 2) {
            jumpVal += Time.fixedDeltaTime / jumpSpeed * 10;
            frameMovement.y -= jumpVal / 2;
        }
        jumpVal = Mathf.Clamp(jumpVal, maxFallSpeed, 2);
        velocityX = move.ReadValue<float>();
        Animate("velocityX", velocityX);
        if (moving && curState != State.Climbing && moveActive) {
            frameMovement.x += velocityX * speedModifier * Time.fixedDeltaTime;
        }

        if (platformBelow != null && platformBelow.moving) {
            float platformMomentum = (platformBelow.endVal - platformBelow.startVal) * platformBelow.speed * Time.fixedDeltaTime;
            frameMovement.x += platformBelow.open ? platformMomentum : -platformMomentum;
        }
        if (curState != State.Jumping && curState != State.Climbing) {
            if (CheckIfGrounded()) {
                animator.ResetTrigger("fall");
                Animate("idle");
                if (!landPlaying && !inWater) {
                    PlaySound("land", actionPitch, 0.2f);
                    landPlaying = true;
                }
            }
        }
        float yMin = curState == State.Grounded ? 0 : -1;
        animator.SetBool("wallBlock", wallLeft || wallRight); //no need for the Animate subroutine, since this is already not run in the same conditions
        if (wallLeft) { //ensures players aren't pressed against the walls (as this causes issues with collisions)
            frameMovement.x += 0.1f;
            wallLeft = false;
        }
        if (wallRight) {
            frameMovement.x -= 0.1f;
            wallRight = false;
        }
        if (waterEnter && curState != State.Grounded) {
            WaterEnterBehaviour();
        }
        if (landPlaying && !audioSource.isPlaying) {
            landPlaying = false;
        }
        if (curState == State.Grounded && Mathf.Abs(frameMovement.x) > 0.1f) {
            if (!audioSource.isPlaying) {
                PlaySound("footsteps", 2 - actionPitch, 0.1f, true);
                walkPlaying = true;
            }
        } else if (walkPlaying) {
            audioSource.Stop();
            walkPlaying = false;
        }
        if (curState == State.Climbing && frameMovement.y != 0) {
            if (!audioSource.isPlaying) {
                float pitch =  1.2f + frameMovement.y * 1.5f;
                PlaySound("ladderStep", pitch, 0, true);
                ladderPlaying = true;
            }
        } else if (ladderPlaying) {
            audioSource.Stop();
            ladderPlaying = false;
        }
        frameMovement.y = Mathf.Clamp(frameMovement.y, yMin, 1);
        transform.Translate(frameMovement);
    }
}
