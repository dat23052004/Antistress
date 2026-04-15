using System.Collections.Generic;
using UnityEngine;

internal sealed class RevealLeafField
{
    internal readonly struct MotionSettings
    {
        public MotionSettings(
            float revealDuration,
            float entryOffset,
            float rotationFromSwipe,
            float randomRotationJitter)
        {
            RevealDuration = revealDuration;
            EntryOffset = entryOffset;
            RotationFromSwipe = rotationFromSwipe;
            RandomRotationJitter = randomRotationJitter;
        }

        public float RevealDuration { get; }
        public float EntryOffset { get; }
        public float RotationFromSwipe { get; }
        public float RandomRotationJitter { get; }
    }

    private sealed class LeafSlot
    {
        public Transform MoveTransform;
        public Transform RotationTransform;
        public SpriteRenderer[] Renderers;
        public Color[] BaseColors;
        public Vector3 RestPosition;
        public Quaternion RestRotation;
        public Vector3 RestScale;
        public bool Revealed;
        public bool FadeFromHidden;
        public Vector3 AnimationStartPosition;
        public Vector3 AnimationTargetPosition;
        public Quaternion AnimationStartRotation;
        public Quaternion TargetRotation;
        public float AnimationElapsed;
    }

    private readonly List<LeafSlot> leafSlots = new List<LeafSlot>();
    private readonly Dictionary<Collider2D, LeafSlot> colliderToLeaf = new Dictionary<Collider2D, LeafSlot>();
    private readonly List<LeafSlot> animatingLeaves = new List<LeafSlot>();

    private Collider2D[] overlapResults = new Collider2D[32];
    private int effectiveLeafMask;

    public bool HasLeaves => leafSlots.Count > 0;

    public void Clear()
    {
        leafSlots.Clear();
        colliderToLeaf.Clear();
        animatingLeaves.Clear();
        effectiveLeafMask = 0;
        overlapResults = new Collider2D[32];
    }

    public void CacheLeaves(Transform leafRoot, LayerMask leafMask)
    {
        Clear();

        if (leafRoot == null)
            return;

        int derivedMask = 0;
        int colliderCount = 0;

        for (int i = 0; i < leafRoot.childCount; i++)
        {
            Transform child = leafRoot.GetChild(i);
            if (child == null)
                continue;

            SpriteRenderer[] renderers = child.GetComponentsInChildren<SpriteRenderer>(true);
            Collider2D[] colliders = child.GetComponentsInChildren<Collider2D>(true);
            if (renderers.Length == 0 || colliders.Length == 0)
                continue;

            LeafSlot slot = new LeafSlot
            {
                MoveTransform = child,
                RotationTransform = colliders[0].transform,
                Renderers = renderers,
                BaseColors = CaptureBaseColors(renderers),
                RestPosition = child.localPosition,
                RestRotation = colliders[0].transform.localRotation,
                RestScale = child.localScale
            };

            leafSlots.Add(slot);

            for (int j = 0; j < colliders.Length; j++)
            {
                Collider2D collider = colliders[j];
                if (collider == null)
                    continue;

                collider.enabled = true;
                colliderToLeaf[collider] = slot;
                derivedMask |= 1 << collider.gameObject.layer;
                colliderCount++;
            }
        }

        effectiveLeafMask = leafMask.value != 0 ? leafMask.value : derivedMask;
        overlapResults = new Collider2D[Mathf.Max(32, colliderCount)];
    }

    public void RevealAtPoint(Vector2 worldPoint, Vector2 swipeDirection, float brushRadius, MotionSettings settings)
    {
        if (effectiveLeafMask == 0)
            return;

        int hitCount = Physics2D.OverlapCircleNonAlloc(worldPoint, brushRadius, overlapResults, effectiveLeafMask);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapResults[i];
            overlapResults[i] = null;

            if (hit == null)
                continue;

            if (!colliderToLeaf.TryGetValue(hit, out LeafSlot slot))
                continue;

            ApplySwipeToLeaf(slot, swipeDirection, settings);
        }
    }

    public void UpdateAnimations(float deltaTime, MotionSettings settings)
    {
        float duration = Mathf.Max(0.01f, settings.RevealDuration);

        for (int i = animatingLeaves.Count - 1; i >= 0; i--)
        {
            LeafSlot slot = animatingLeaves[i];
            if (slot == null || slot.MoveTransform == null || slot.RotationTransform == null)
            {
                animatingLeaves.RemoveAt(i);
                continue;
            }

            slot.AnimationElapsed += deltaTime;
            float progress = Mathf.Clamp01(slot.AnimationElapsed / duration);
            float eased = 1f - Mathf.Pow(1f - progress, 3f);

            slot.MoveTransform.localPosition =
                Vector3.LerpUnclamped(slot.AnimationStartPosition, slot.AnimationTargetPosition, eased);
            slot.RotationTransform.localRotation =
                Quaternion.SlerpUnclamped(slot.AnimationStartRotation, slot.TargetRotation, eased);

            if (slot.FadeFromHidden)
            {
                for (int j = 0; j < slot.Renderers.Length; j++)
                {
                    SpriteRenderer renderer = slot.Renderers[j];
                    if (renderer == null)
                        continue;

                    Color baseColor = slot.BaseColors[j];
                    renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * eased);
                }
            }

            if (progress < 1f)
                continue;

            slot.FadeFromHidden = false;
            slot.MoveTransform.localPosition = slot.RestPosition;
            slot.RotationTransform.localRotation = slot.TargetRotation;
            animatingLeaves.RemoveAt(i);
        }
    }

    public void HideAllLeaves()
    {
        animatingLeaves.Clear();

        for (int i = 0; i < leafSlots.Count; i++)
        {
            LeafSlot slot = leafSlots[i];
            slot.Revealed = false;
            slot.FadeFromHidden = false;
            slot.AnimationElapsed = 0f;
            slot.MoveTransform.localPosition = slot.RestPosition;
            slot.RotationTransform.localRotation = slot.RestRotation;
            slot.MoveTransform.localScale = slot.RestScale;

            for (int j = 0; j < slot.Renderers.Length; j++)
            {
                SpriteRenderer renderer = slot.Renderers[j];
                if (renderer == null)
                    continue;

                Color baseColor = slot.BaseColors[j];
                renderer.enabled = false;
                renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            }
        }
    }

    private static Color[] CaptureBaseColors(SpriteRenderer[] renderers)
    {
        Color[] colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            colors[i] = renderers[i].color;

        return colors;
    }

    private void ApplySwipeToLeaf(LeafSlot slot, Vector2 swipeDirection, MotionSettings settings)
    {
        Vector2 direction = swipeDirection.sqrMagnitude > 0.0001f ? swipeDirection.normalized : Vector2.zero;
        bool firstReveal = !slot.Revealed;

        if (!firstReveal && direction == Vector2.zero)
            return;

        slot.Revealed = true;
        slot.FadeFromHidden = firstReveal;
        slot.AnimationElapsed = 0f;
        slot.AnimationStartPosition = firstReveal
            ? slot.RestPosition - (Vector3)(direction * settings.EntryOffset)
            : slot.MoveTransform.localPosition;
        slot.AnimationTargetPosition = slot.RestPosition;
        slot.AnimationStartRotation = slot.RotationTransform.localRotation;
        slot.TargetRotation = GetSwipeRotation(slot, direction, firstReveal, settings.RotationFromSwipe);

        if (firstReveal)
        {
            slot.AnimationStartRotation *= Quaternion.Euler(
                0f,
                0f,
                Random.Range(-settings.RandomRotationJitter, settings.RandomRotationJitter));
        }

        slot.MoveTransform.localPosition = slot.AnimationStartPosition;
        slot.RotationTransform.localRotation = slot.AnimationStartRotation;
        slot.MoveTransform.localScale = slot.RestScale;

        for (int i = 0; i < slot.Renderers.Length; i++)
        {
            SpriteRenderer renderer = slot.Renderers[i];
            if (renderer == null)
                continue;

            Color baseColor = slot.BaseColors[i];
            renderer.enabled = true;
            renderer.color = firstReveal
                ? new Color(baseColor.r, baseColor.g, baseColor.b, 0f)
                : baseColor;
        }

        if (!animatingLeaves.Contains(slot))
            animatingLeaves.Add(slot);
    }

    private static Quaternion GetSwipeRotation(
        LeafSlot slot,
        Vector2 swipeDirection,
        bool firstReveal,
        float rotationFromSwipe)
    {
        if (swipeDirection == Vector2.zero)
            return firstReveal ? slot.RestRotation : slot.RotationTransform.localRotation;

        float worldAngle = Vector2.SignedAngle(Vector2.up, swipeDirection) + rotationFromSwipe;
        Quaternion worldRotation = Quaternion.Euler(0f, 0f, worldAngle);
        Transform parent = slot.RotationTransform.parent;
        Quaternion parentRotation = parent != null ? parent.rotation : Quaternion.identity;
        return Quaternion.Inverse(parentRotation) * worldRotation;
    }
}
