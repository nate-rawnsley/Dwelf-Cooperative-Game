using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//Controls the Dwarf character, adding support for axe swinging & throwing, tunelling, drowning and being held/bowled.
//Adds an additional enum so that activity can be overridden without setting the character inactive.

public class DwarfControls : PlayerController {
    [SerializeField] private GameObject projectileAxe;
    private GameObject pAxe;
    private AxeProjectile axeScript;
    private Transform heldAxe;
    private bool swinging;

    private float currentSwingAngle;
    private float endSwingAngle;
    private bool swingPositive;
    private float startRotation;
    private float rotationIncrement;
    private bool rollPlaying;

    private float AXE_RADIUS = 0.75f;
    private float RADIANS_TO_DEGREES;

    public enum DwarfState { Normal, Held, Rolling, Tunelling, Sinking };
    public DwarfState dwarfState;
    public bool rollRight;
    public float struggleDir;
    public int struggleCount;

    private bool holdingAxe = true;
    public bool hasAxe = true;
    private bool axeThrowBuffer = false;
    private Animator axeAnim;

    protected override void Awake() {
        base.Awake();
        speedModifier = 8;
        jumpSpeed = 12;
        jumpStartSpeed = 0.6f;
        GameObject obj = Instantiate(projectileAxe);
        pAxe = obj;
        axeScript = pAxe.GetComponent<AxeProjectile>();
        actionPitch = 0.8f;
    }

    protected override void PlaySound(string soundName, float pitch, float randomRange = 0, bool loop = false, float volumeMod = 1) {
        if (!loop) {
            rollPlaying = false;
        }
        base.PlaySound(soundName, pitch, randomRange, loop, volumeMod);
    }

    protected override void OnMove(InputValue value) {
        if (dwarfState == DwarfState.Normal) {
            base.OnMove(value);
        } else if (dwarfState == DwarfState.Held) {
            float dir = Mathf.Clamp(value.Get<float>(), -0.1f, 0.1f);
            if ((struggleDir > 0 && dir < 0) || (struggleDir < 0 && dir > 0) || (struggleDir == 0 && dir != 0)) {
                struggleDir = dir;
                struggleCount++;
                PlaySound("projectileLand", 0.75f, 0.2f, false, 0.25f);
            }
        } else {
            return;
        }
    }

    protected override void OnJump() {
        if (dwarfState != DwarfState.Sinking) {
            base.OnJump();
        }
    }

    protected override void OnJumpEnd() {
        if (dwarfState != DwarfState.Sinking) {
            base.OnJumpEnd();
        }
    }

    protected override void OnClimb(InputValue value) {
        if (dwarfState == DwarfState.Normal) {
            base.OnClimb(value);
        }
    }

    private void Start() {
        heldAxe = transform.Find("Axe");
        if (!hasAxe) {
            heldAxe.gameObject.SetActive(false);
        }
        axeAnim = heldAxe.gameObject.GetComponent<Animator>();
        RADIANS_TO_DEGREES = 180 / Mathf.PI;
        SwitchAimControls();
    }

    public void SwitchAimControls() { //also called from ControlSettings
        aimPos = input.actions.FindAction("Aim");
    }

    private void OnAxeThrow(InputValue value) {
        if (!holdingAxe || !hasAxe) {
            axeThrowBuffer = value.Get<float>() > 0;
            return;
        }
        if (value.Get<float>() > 0) {
            aiming = true;
            lineLength = 2;
            projectileAcceleration = 0.5f;
            swinging = false;
            speedModifier = 8;
            Animate("idle");
        } else {
            aiming = false;
            Vector3 axePos = heldAxe.position;
            axePos.z -= 0.5f;
            pAxe.transform.position = axePos;
            pAxe.transform.rotation = heldAxe.rotation;
            heldAxe.gameObject.SetActive(false);
            axeScript.StartFiring((lineLength * 0.75f) * aimingVector);
            holdingAxe = false;
            lineRenderer.positionCount = 0;
            PlaySound("axeSwing", 0.7f, 0.1f);
        }
    }

    public void PauseAxe() {
        axeScript.StopReturn();
    }

    public void ResumeAxe() {
        axeScript.ResumeReturn();
    }

    private void OnAxeReturn() {
        if (!holdingAxe && hasAxe && dwarfState != DwarfState.Held && dwarfState != DwarfState.Rolling) {
            axeScript.SwitchBehaviour(transform);
        }
    }

    private void OnAxe(InputValue value) {
        if (Time.timeScale < 1 || dwarfState != DwarfState.Normal || !holdingAxe || swinging || !hasAxe) {
            return;
        }
        if (value.Get<float>() <= 0) {
            return;
        }
        swinging = true;
        swingPositive = aimingVector.x > 0;

        float angle = Mathf.Atan2(aimingVector.y, aimingVector.x) * RADIANS_TO_DEGREES;
        angle = angle < 0 ? angle + 360 : angle;

        if (swingPositive) {
            currentSwingAngle = angle + 50;
            endSwingAngle = angle - 50;
        } else {
            currentSwingAngle = angle - 50;
            endSwingAngle = angle + 50;
        }

        currentSwingAngle /= 60;
        endSwingAngle /= 60;

        startRotation = transform.localRotation.z;
        rotationIncrement = 0;

        animator.SetBool("swingDir", swingPositive);
        animator.SetTrigger("swing");

        PlaySound("axeSwing", 1, 0.1f);

        heldAxe.gameObject.layer = LayerMask.NameToLayer("NoCollision");
        speedModifier = 0;
    }

    private void AxeSwing() {
        Vector3 incrementPos;
        incrementPos.x = Mathf.Cos(currentSwingAngle);
        incrementPos.y = Mathf.Sin(currentSwingAngle);
        incrementPos.z = -0.1f;

        heldAxe.localPosition = incrementPos * AXE_RADIUS * 1.1f;
        heldAxe.localRotation = Quaternion.Euler(0, 0, startRotation + rotationIncrement);
        currentSwingAngle += swingPositive ? Time.fixedDeltaTime * -6 : Time.fixedDeltaTime * 6;
        rotationIncrement += swingPositive ? Time.fixedDeltaTime * -210 : Time.fixedDeltaTime * 210;

        bool curLessThanEnd = currentSwingAngle < endSwingAngle;
        if (curLessThanEnd == swingPositive) {
            swinging = false;
            heldAxe.gameObject.tag = "AxeStatic";
            speedModifier = 8;
            Animate("idle");

            Vector3 tempPos = heldAxe.localPosition;
            tempPos.z = 0.1f;
            heldAxe.localPosition = tempPos;
            heldAxe.gameObject.layer = LayerMask.NameToLayer("HeldAxe");
        }
    }

    protected override void OnPause() {
        if (Time.timeScale > 0) {
            rollPlaying = false;
        }
        base.OnPause();
    }

    private void OnTunnel() {
        if (curState == State.Climbing || dwarfState != DwarfState.Normal) {
            return;
        }
        int interactMask = LayerMask.GetMask("Interactable");
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, 0.5f, interactMask);
        foreach (var hitObject in hitObjects){
            if (hitObject != null && hitObject.CompareTag("Tunnel")) {
                var tunnelPos = hitObject.transform.position;
                tunnelPos.z = transform.position.z;
                transform.position = tunnelPos;
                hitObject.gameObject.GetComponent<Tunnel>().DwarfTunnel(gameObject);
                dwarfState = DwarfState.Tunelling;
                PlaySound("footsteps", 0.4f, 0);
            }
        }
    }

    protected override void WaterEnterBehaviour() {
        base.WaterEnterBehaviour();
        dwarfState = DwarfState.Sinking;
        Animate("sink");
        PlaySound("drowning", 1);
        speedModifier = 1;
    }

    protected override void WaterExitBehaviour() {
        Animate("waterExit");
        Animate("jump");
        base.WaterExitBehaviour();
        if (dwarfState != DwarfState.Rolling) {
            dwarfState = DwarfState.Normal;
            speedModifier = 8;
        }
    }

    public void AxeReturned() {
        if (!hasAxe) {
            return;
        }
        holdingAxe = true;
        heldAxe.gameObject.SetActive(true);
        if (axeThrowBuffer) {
            aiming = true;
            lineLength = 2;
            projectileAcceleration = 0.5f;
            swinging = false;
            speedModifier = 8;
            Animate("idle");
        }
    }

    private void GetAxe() {
        Animate("pickup");
        StopRolling();
        StartCoroutine(WaitForAxe());
    }

    private IEnumerator WaitForAxe() {
        active = false;
        yield return new WaitForSeconds(5);
        active = true;
        hasAxe = true;
        heldAxe.gameObject.SetActive(true);
    }

    private void StopRolling() {
        dwarfState = DwarfState.Normal;
        moveActive = true;
        if (hasAxe) {
            heldAxe.gameObject.SetActive(true);
        }
        animator.ResetTrigger("jump");
        animator.SetBool("bowling", false);
        Vector2 collideSize = GetComponent<BoxCollider2D>().size;
        collideSize.y = 1.97f;
        GetComponent<BoxCollider2D>().size = collideSize;
        playerHeight = 1;
        ResumeAxe();
    }

    protected override void FixedUpdate() {
        switch (dwarfState) {
            case DwarfState.Held:
                return;
            case DwarfState.Sinking:
                if (curState == State.Grounded) {
                    Vector3 submergeCheckPos = transform.position;
                    submergeCheckPos.y += playerHeight / 2;
                    if (RayCheck(submergeCheckPos, Vector3.up, 1 << LayerMask.NameToLayer("Water"))) {
                        OnDeath();
                    }
                }
                break;
            case DwarfState.Rolling:
                if (!audioSource.isPlaying) {
                    PlaySound("rolling", 1, 0, true);
                    rollPlaying = true;
                }
                if ((rollRight && wallRight) || (!rollRight && wallLeft)) {
                    StopRolling();
                    PlaySound("projectileLand", 0.5f, 0, false, 1.2f);
                } else {
                    Vector3 rollDist = new Vector3();
                    rollDist.x = rollRight ? 7.5f : -7.5f;
                    rollDist.x *= Time.fixedDeltaTime;
                    transform.Translate(rollDist);
                }
                break;
        }
        base.FixedUpdate();
        if (dwarfState != DwarfState.Rolling && rollPlaying) {
            audioSource.Stop();
            rollPlaying = false;
        }
        if (Time.timeScale < 1 || dwarfState == DwarfState.Rolling || !hasAxe) {
            return;
        }
        aimingVector = aimPos.ReadValue<Vector2>();
        if (aimingVector.x > 0) {
            aimingVector.x = Mathf.Clamp(aimingVector.x, 0.35f, 1);
        } else if (aimingVector.x < 0) {
            aimingVector.x = Mathf.Clamp(aimingVector.x, -1, -0.35f);
        } else {
            aimingVector.x = velocityX;
        }
        
        aimingVector.y = Mathf.Clamp(aimingVector.y, -0.7f, 0.75f);
        if (aiming) {
            DrawTrajectory(false, 1, 24);
        }
        if (swinging && holdingAxe && !aiming) {
            AxeSwing();
        } else if (aimingVector.x != 0 && aimingVector.y != 0 && holdingAxe) {
            heldAxe.gameObject.SetActive(true);
            heldAxe.localPosition = aimingVector * AXE_RADIUS;
            float rotationFactor = aimingVector.x >= 0 ? 15 : -15;
            heldAxe.localRotation = Quaternion.Euler(transform.forward * aimingVector.y * rotationFactor);
        } else {
            heldAxe.gameObject.SetActive(false);
        }
    }
}
