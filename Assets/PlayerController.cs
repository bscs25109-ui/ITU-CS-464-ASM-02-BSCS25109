using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform feet;
    [SerializeField] private LayerMask groundMask;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.8f;

    [Header("Ground Check")]
    [SerializeField] private float checkRadius = 0.2f;

    [Header("Jump Feel")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBuffer = 0.12f;
    [SerializeField] private float fallMultiplier = 2.2f;

    private Rigidbody rb;
    private Vector3 move;
    private bool isGrounded;
    private bool jumpQueued;

    private float lastGroundedTime = float.NegativeInfinity;
    private float lastPressedTime = float.NegativeInfinity;
    private float ignoreGroundUntil = float.NegativeInfinity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // Read all input here.
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();
        }

        // Camera-relative movement; no faster diagonals.
        move = Vector3.ClampMagnitude(
            forward * v + right * h, 1f
        );

        isGrounded =
            feet != null &&
            Time.time >= ignoreGroundUntil &&
            rb.linearVelocity.y <= 0.1f &&
            Physics.CheckSphere(
                feet.position,
                checkRadius,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

        if (isGrounded && !jumpQueued)
            lastGroundedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump =
            Time.time - lastGroundedTime <= coyoteTime;

        bool wantsJump =
            Time.time - lastPressedTime <= jumpBuffer;

        if (!jumpQueued && canJump && wantsJump)
        {
            jumpQueued = true;

            // Consume both windows.
            lastGroundedTime = float.NegativeInfinity;
            lastPressedTime = float.NegativeInfinity;
        }
    }

    void FixedUpdate()
    {
        // Velocity is already metres per second.
        rb.linearVelocity = new Vector3(
            move.x * speed,
            rb.linearVelocity.y,
            move.z * speed
        );

        if (jumpQueued)
        {
            // Remove existing vertical motion for a consistent jump.
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            float gravity = -Physics.gravity.y;
            float launchSpeed =
                Mathf.Sqrt(2f * gravity * jumpHeight);

            rb.AddForce(
                Vector3.up * rb.mass * launchSpeed,
                ForceMode.Impulse
            );

            jumpQueued = false;
            isGrounded = false;

            // Prevent the feet sphere from counting the takeoff
            // surface again immediately after launching.
            ignoreGroundUntil = Time.time + 0.1f;
        }
        else if (rb.linearVelocity.y < 0f)
        {
            rb.AddForce(
                Physics.gravity * (fallMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;

        Gizmos.color = isGrounded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}