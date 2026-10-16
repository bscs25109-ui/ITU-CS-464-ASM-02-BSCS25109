using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Rigidbody targetBody;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 4f, -7f);
    [SerializeField] private float sharpness = 5f;
    [SerializeField] private float lookAhead = 0.35f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Assign the Player to CameraFollow's Target.", this);
            enabled = false;
            return;
        }

        if (targetBody == null)
            targetBody = target.GetComponent<Rigidbody>();

        // Start at the player instead of travelling from the editor position.
        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    void LateUpdate()
    {
        Vector3 ahead = Vector3.zero;

        if (targetBody != null)
        {
            Vector3 velocity = targetBody.linearVelocity;
            velocity.y = 0f;
            ahead = velocity * lookAhead;
        }

        Vector3 wanted = target.position + offset + ahead;

        // Consistent exponential smoothing across frame rates.
        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            wanted,
            blend
        );

        // Look toward the same point we are following.
        transform.LookAt(target.position + ahead);
    }
}