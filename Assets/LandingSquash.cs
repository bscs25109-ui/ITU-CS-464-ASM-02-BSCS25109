using System.Collections;
using UnityEngine;

public class LandingSquash : MonoBehaviour
{
    [SerializeField] Transform body;
    [SerializeField] float duration = 0.2f;

    PlayerController player;
    Vector3 originalScale;
    bool wasGrounded;
    bool playing;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        originalScale = body.localScale;
    }

    void LateUpdate()
    {
        bool grounded = player.IsGrounded;

        if (grounded && !wasGrounded && player.HasJumped)
        {
            player.ClearJump();

            if (!playing)
                StartCoroutine(Squash());
        }

        wasGrounded = grounded;
    }

    IEnumerator Squash()
    {
        playing = true;

        Vector3 squashed = Vector3.Scale(
            originalScale, new Vector3(1.4f, 0.6f, 1.4f)
        );

        for (float elapsed = 0f; elapsed < duration;
             elapsed += Time.deltaTime)
        {
            float t = elapsed / duration;
            float eased = 1f - (1f - t) * (1f - t);

            body.localScale =
                Vector3.Lerp(squashed, originalScale, eased);

            yield return null;
        }

        body.localScale = originalScale;
        playing = false;
    }
}