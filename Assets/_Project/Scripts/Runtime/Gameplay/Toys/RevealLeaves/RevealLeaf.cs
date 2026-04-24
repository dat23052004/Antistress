using UnityEngine;

[DisallowMultipleComponent]
public sealed class RevealLeaf : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform rotationTransform;

    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;

    private bool revealed;
    private bool isAnimating;
    private float animationElapsed;

    private Vector3 animationStartPosition;
    private Quaternion animationStartRotation;
    private Quaternion targetRotation;

    public bool HasRequiredReferences => hitCollider != null && spriteRenderer != null;

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void OnValidate()
    {
        AutoAssignReferences();
    }

    public bool Prepare()
    {
        if (!HasRequiredReferences)
            return false;

        if (rotationTransform == null)
            rotationTransform = transform;

        restLocalPosition = transform.localPosition;
        restLocalRotation = rotationTransform.localRotation;
        hitCollider.enabled = true;
        return true;
    }

    public void HideInstant()
    {
        revealed = false;
        isAnimating = false;
        animationElapsed = 0f;

        transform.localPosition = restLocalPosition;
        rotationTransform.localRotation = restLocalRotation;
        spriteRenderer.enabled = false;
    }

    public bool MatchesCollider(Collider2D collider)
    {
        return collider != null && collider == hitCollider;
    }

    public bool Reveal(
        Vector2 swipeDirection,
        float entryOffset,
        float rotationFromSwipe,
        float randomRotationJitter)
    {
        Vector2 direction = swipeDirection.sqrMagnitude > 0.0001f
            ? swipeDirection.normalized
            : Vector2.zero;

        bool firstReveal = !revealed;
        if (!firstReveal && direction == Vector2.zero)
            return false;

        revealed = true;
        animationElapsed = 0f;

        animationStartPosition = firstReveal
            ? restLocalPosition - (Vector3)(direction * entryOffset)
            : transform.localPosition;

        Quaternion startRotation = rotationTransform.localRotation;
        if (firstReveal)
        {
            startRotation *= Quaternion.Euler(
                0f,
                0f,
                Random.Range(-randomRotationJitter, randomRotationJitter));
        }

        animationStartRotation = startRotation;
        targetRotation = GetSwipeRotation(direction, firstReveal, rotationFromSwipe);

        transform.localPosition = animationStartPosition;
        rotationTransform.localRotation = animationStartRotation;
        spriteRenderer.enabled = true;

        bool startedAnimatingNow = !isAnimating;
        isAnimating = true;
        return startedAnimatingNow;
    }

    public bool TickAnimation(float deltaTime, float revealDuration)
    {
        if (!isAnimating)
            return false;

        float duration = Mathf.Max(0.01f, revealDuration);

        animationElapsed += deltaTime;
        float progress = Mathf.Clamp01(animationElapsed / duration);
        float eased = 1f - Mathf.Pow(1f - progress, 3f);

        transform.localPosition =
            Vector3.LerpUnclamped(animationStartPosition, restLocalPosition, eased);

        rotationTransform.localRotation =
            Quaternion.SlerpUnclamped(animationStartRotation, targetRotation, eased);

        if (progress < 1f)
            return true;

        isAnimating = false;
        transform.localPosition = restLocalPosition;
        rotationTransform.localRotation = targetRotation;
        return false;
    }

    private Quaternion GetSwipeRotation(Vector2 swipeDirection, bool firstReveal, float rotationFromSwipe)
    {
        if (swipeDirection == Vector2.zero)
            return firstReveal ? restLocalRotation : rotationTransform.localRotation;

        float worldAngle = Vector2.SignedAngle(Vector2.up, swipeDirection) + rotationFromSwipe;
        Quaternion worldRotation = Quaternion.Euler(0f, 0f, worldAngle);

        Transform parent = rotationTransform.parent;
        Quaternion parentRotation = parent != null ? parent.rotation : Quaternion.identity;

        return Quaternion.Inverse(parentRotation) * worldRotation;
    }

    private void AutoAssignReferences()
    {
        if (rotationTransform == null)
        {
            Transform bodyTransform = transform.Find("Body");
            rotationTransform = bodyTransform != null ? bodyTransform : transform;
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (hitCollider == null)
        {
            if (rotationTransform != null && rotationTransform.TryGetComponent(out Collider2D bodyCollider))
                hitCollider = bodyCollider;
            else
                hitCollider = GetComponentInChildren<Collider2D>(true);
        }
    }
}
