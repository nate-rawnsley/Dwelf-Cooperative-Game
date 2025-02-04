using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//The controller for the elf, adding support for shooting the bow and picking up the dwarf.
//Speed and jump is changd based on action, so default and reduced values are set.

public class ElfControls : PlayerController {
    [SerializeField] private GameObject arrow;

    private GameObject[] arrows = new GameObject[10];
    private Arrow[] arrowScripts = new Arrow[10];
    private int arrowIndex;

    private GameObject heldObject; //currently just the dwarf but I'm leaving it open for more
    private DwarfControls dControls;
    private float pickupCooldownEnd;

    float jumpSpeedDefault = 17.5f, jumpStartDefault = 0.785f, speedDefault = 6.5f;
    float jumpSpeedReduced = 12, jumpStartReduced = 0.6f, speedReduced = 3;

    private Animator dwarfAnimator;

    public bool hasBow = true;

    protected override void Awake() {
        base.Awake();
        speedModifier = speedDefault;
        jumpSpeed = jumpSpeedDefault;
        jumpStartSpeed = jumpStartDefault;
        //Spawns arrows at the start, for performance.
        for (int i = 0; i < arrows.Length; i++) {
            GameObject newArrow = Instantiate(arrow);
            arrows[i] = newArrow;
            arrowScripts[i] = newArrow.GetComponent<Arrow>();
        }
        Animate("aimAngle", -1);
        actionPitch = 1.2f;
    }

    protected override void OnPause() {
        OnBowEnd();
        base.OnPause();
    }

    private void OnBow() {
        if (Time.timeScale < 1 || heldObject != null || !hasBow) { //Don't run behaviour when the game is paused or dwarf held.
            return;
        }
        aimPos = input.actions.FindAction("Aim");
        aiming = true;
        lineLength = 2;
        projectileAcceleration = 0.5f;
        speedModifier = 0;
        Animate("bowTrigger");
        animator.SetBool("bow", true);
        PlaySound("bowDraw", 1, 0.075f);
    }

    private void OnBowEnd() {
        if (Time.timeScale < 1 || heldObject != null || !hasBow || !aiming) {
            return;
        }
        aiming = false;
        Vector3 arrowSpawnPos = transform.position;
        arrowSpawnPos.z += 0.1f;
        arrows[arrowIndex].transform.position = arrowSpawnPos;
        arrowScripts[arrowIndex].StartFiring(lineLength * aimingVector);
        arrowIndex = (arrowIndex + 1) % arrows.Length;
        lineRenderer.positionCount = 0;
        speedModifier = 6.5f;
        Animate("aimAngle", -1);
        animator.SetBool("bow", false);
        Animate("idle");
        PlaySound("bowRelease", 1, 0.175f);
    }

    private bool CheckForDwarf(Vector3 checkPos) {
        bool found = false;
        int interactMask = LayerMask.GetMask("Player");
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(checkPos, 1, interactMask);
        foreach (var hitObject in hitObjects) {
            if (hitObject != null && hitObject.CompareTag("Dwarf")) {
                dControls = hitObject.GetComponent<DwarfControls>();
                if (dControls.dwarfState == DwarfControls.DwarfState.Normal || dControls.dwarfState == DwarfControls.DwarfState.Sinking && dControls.active) {
                    dControls.dwarfState = DwarfControls.DwarfState.Held;
                    dControls.PauseAxe();
                    hitObject.GetComponent<Rigidbody2D>().simulated = false;
                    hitObject.GetComponent<AudioSource>().Stop();
                    hitObject.transform.Find("Axe").gameObject.SetActive(false);
                    dwarfAnimator = hitObject.GetComponent<Animator>();
                    dwarfAnimator.SetBool("held", true);
                    heldObject = hitObject.gameObject;
                    jumpSpeed = jumpSpeedReduced;
                    jumpStartSpeed = jumpStartReduced;
                    speedModifier = speedReduced;
                    found = true;
                    animator.SetBool("holdDwarf", true);
                }
            }
        }
        return found;
    }

    private void OnPickupDwarf() {
        if (pickupCooldownEnd > Time.time) {
            return;
        }
        Vector3 leftCheck = transform.position;
        leftCheck.x -= 1;
        if (CheckForDwarf(leftCheck)) {
            //room for left pickup animation - unused.
        }
        Vector3 rightCheck = transform.position;
        rightCheck.x += 1;
        if (CheckForDwarf(rightCheck)) {
            //room for right pickup animation - unused.
        }
    }

    private void DropDwarf() {
        dControls.dwarfState = DwarfControls.DwarfState.Normal;
        dwarfAnimator.SetBool("held", false);
        dwarfAnimator.SetTrigger("idle");
        heldObject.GetComponent<Rigidbody2D>().simulated = true;
        heldObject = null;
        animator.SetBool("holdDwarf", false);
        Animate("idle");
        speedModifier = speedDefault;
        jumpSpeed = jumpSpeedDefault;
        jumpStartSpeed = jumpStartDefault;
        dControls.struggleCount = 0;
        dControls.struggleDir = 0;
        pickupCooldownEnd = Time.time + 0.5f;
        dControls.ResumeAxe();
    }

    private void OnBowlDwarf() {
        if (heldObject == null) { //If not holding dwarf return
            return;
        }
        if (velocityX == 0) { //If not moving, just drop
            DropDwarf();
            return;
        }
        heldObject.GetComponent<Rigidbody2D>().simulated = true;
        dControls.dwarfState = DwarfControls.DwarfState.Rolling;
        bool rollRight = velocityX > 0;
        dControls.rollRight = rollRight;
        dwarfAnimator.SetBool("rollDir", rollRight);
        dwarfAnimator.SetTrigger("roll");
        dwarfAnimator.SetBool("bowling", true);
        dwarfAnimator.SetBool("held", false);
        animator.SetBool("holdDwarf", false);
        Animate("idle");
        Vector3 releasePos = transform.position;
        releasePos.y -= 1;
        releasePos.x += rollRight ? 0.15f : -0.15f;
        heldObject.transform.position = releasePos;
        Vector2 collideSize = heldObject.GetComponent<BoxCollider2D>().size;
        collideSize.y = 1.4f;
        heldObject.GetComponent<BoxCollider2D>().size = collideSize;
        dControls.playerHeight = 0.7f;
        dControls.moveActive = false;
        heldObject = null;
        speedModifier = speedDefault;
        jumpSpeed = jumpSpeedDefault;
        jumpStartSpeed = jumpStartDefault;
    }

    protected override void WaterEnterBehaviour() {
        base.WaterEnterBehaviour();
        jumpSpeed = 42.5f;
        jumpStartSpeed = jumpStartReduced;
        
    }

    protected override void WaterExitBehaviour() {
        base.WaterExitBehaviour();
        jumpSpeed = jumpSpeedDefault;
        jumpStartSpeed = jumpStartDefault;
    }

    private void GetBow() {
        Animate("pickup");
        StartCoroutine(WaitForBow());
    }

    private IEnumerator WaitForBow() {
        active = false;
        yield return new WaitForSeconds(5);
        active = true;
        hasBow = true;
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();
        if (Time.timeScale < 1) {
            return;
        }
        if (aiming) {
            aimingVector = aimPos.ReadValue<Vector2>();
            DrawTrajectory(true, 1.38f, 30);
        }
        if (heldObject != null) {
            Vector3 heldPos = transform.position;
            float heldOffset = velocityX > 0 ? 1.3f : velocityX < 0 ? -1.5f : 0;
            heldPos.x += heldOffset + dControls.struggleDir;
            heldPos.z -= 0.1f;
            dwarfAnimator.SetFloat("heldDir", heldOffset);
            heldObject.transform.position = heldPos;
            if (dControls.struggleCount >= 10) {
                DropDwarf();
            }
        }
    }
}
