
using System.Collections; 
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f;  // metres
    [SerializeField] float coyoteTime = 0.10f;
    [SerializeField] float jumpBuffer = 0.12f;
    [SerializeField] float fallMultiplier = 2.2f;
    Rigidbody rb;
    Transform cam;
    Vector3 move;          // built in Update, consumed in FixedUpdate
    bool isGrounded;
    bool jumpQueued;
    float lastGroundedTime = -1f;
    float lastPressedTime = -1f;

    [SerializeField] Transform body;
    Coroutine squashRoutine;

    float airTime;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.transform;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // Read keys every frame — never miss a press
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Camera-relative directions (Part C — works from the start)
        Vector3 fwd = cam.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 rgt = cam.right; rgt.y = 0f; rgt.Normalize();
        move = Vector3.ClampMagnitude(fwd * v + rgt * h, 1f);

        // Coyote: stamp ground time (only when not rising/queued) 
        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;

        //  Buffer: stamp press time 
        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        //  Both windows open → queue jump
        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;
        if (canJump && wantJump)
        {
            jumpQueued = true;
            lastGroundedTime = -1f;  // consume both windows
            lastPressedTime = -1f;
        }

        // ── Air time (for juice trigger) 
        if (!isGrounded) airTime += Time.deltaTime;
        else airTime = 0f;
    }

    void FixedUpdate()
    {
        // Set horizontal velocity, keep current y (gravity/jump)
        rb.linearVelocity = new Vector3(
            move.x * speed,
            rb.linearVelocity.y,
            move.z * speed);

        // Apply queued jump once
        if (jumpQueued)
        {
            // Zero downward velocity so coyote jumps are full height
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);

            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
            jumpQueued = false;
        }

        // Fall gravity 
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f),
                        ForceMode.Acceleration);

    }

    // Ground check — Option A: contact normal
    void OnCollisionStay(Collision c)
    {
        isGrounded = c.GetContact(0).normal.y > 0.5f;
    }
    void OnCollisionExit(Collision c) => isGrounded = false;
    void OnCollisionEnter(Collision c)
    {
        // Only squash on a real landing: surface is up, and was airborne > 0.1s
        if (c.GetContact(0).normal.y > 0.5f && airTime > 0.1f)
        {
            if (squashRoutine != null) StopCoroutine(squashRoutine);
            squashRoutine = StartCoroutine(SquashAnim());
        }
    }

    IEnumerator SquashAnim()
    {
        float duration = 0.25f;  // 15 frames at 60fps
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / duration);
            float yScale = Mathf.LerpUnclamped(0.7f, 1f, p);
            float xzScale = Mathf.LerpUnclamped(1.195f, 1f, p);
            if (body != null)
                body.localScale = new Vector3(xzScale, yScale, xzScale);
            yield return null;
        }
        if (body != null)
            body.localScale = Vector3.one;
        squashRoutine = null;
    }
    // Draw ground contact in Scene view when player is selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.1f);
    }
}