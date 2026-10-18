
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0, 4, -7);
    [SerializeField] float sharpness = 5f;
    [SerializeField] float lookAhead = 0.35f;

    Rigidbody targetRb;

    void Awake()
    {
        // Find player automatically if not assigned
        if (target == null)
        {
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc) target = pc.transform;
        }
        if (target) targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()   // runs AFTER all Update() calls
    {
        if (target == null) return;

        // ① Where do we want to look?
        Vector3 aimPoint = target.position;

        // ② Add look-ahead: aim where player will be in 0.35s
        if (targetRb != null)
        {
            Vector3 flatVel = targetRb.linearVelocity;
            flatVel.y = 0f;
            aimPoint += flatVel * lookAhead;
        }

        // ③ Camera sits at aimPoint + offset
        Vector3 wanted = aimPoint + offset;

        // ④ Exponential smoothing: same curve at any framerate
        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, wanted, blend);

        // ⑤ Look at the aim point (not the player) so look-ahead works
        transform.LookAt(aimPoint);
    }
}