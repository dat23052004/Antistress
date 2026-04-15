using UnityEngine;

public class RevealController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Transform leafRoot;
    [SerializeField] private GameObject leafPrefab;

    [Header("Layout")]
    [SerializeField] private float spawnSpacingX = 0.5f;
    [SerializeField] private float spawnSpacingY = 0.6f;
    [SerializeField] private bool staggerOddRows = true;
    [SerializeField] private float rowOffsetX = 0.25f;
    [SerializeField] private float overscanX = 0.25f;
    [SerializeField] private float overscanY = 0.3f;

    [Header("Detection")]
    [SerializeField] private LayerMask leafMask;
    [SerializeField] private float brushRadius = 0.3f;
    [SerializeField] private float sampleSpacing = 0.12f;
    [SerializeField] private float minSwipeDistance = 0.01f;
    [SerializeField] private bool blockInputOverUI = true;

    [Header("Reveal Motion")]
    [SerializeField] private float revealDuration = 0.12f;
    [SerializeField] private float entryOffset = 0.18f;
    [SerializeField] private float rotationFromSwipe = 16f;
    [SerializeField] private float randomRotationJitter = 6f;

    private readonly RevealGridBuilder gridBuilder = new RevealGridBuilder();
    private readonly RevealLeafField leafField = new RevealLeafField();

    private Camera mainCamera;
    private bool isSwiping;
    private Vector2 lastPointerWorld;
    private Vector2 lastSampleWorld;

    private void Awake()
    {
        ResolveReferences();
        ResetReveal();
    }

    private void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        RevealLeafField.MotionSettings motionSettings = CreateMotionSettings();
        HandleInput(motionSettings);
        leafField.UpdateAnimations(Time.deltaTime, motionSettings);

        if (InputManager.ResetPressedThisFrame())
            ResetReveal();
    }

    private void ResolveReferences()
    {
        if (backgroundRenderer == null)
        {
            backgroundRenderer = GetComponent<SpriteRenderer>();
            if (backgroundRenderer == null)
                backgroundRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        leafRoot = gridBuilder.ResolveLeafRoot(transform, leafRoot);
    }

    private RevealGridBuilder.LayoutSettings CreateLayoutSettings()
    {
        return new RevealGridBuilder.LayoutSettings(
            spawnSpacingX,
            spawnSpacingY,
            staggerOddRows,
            rowOffsetX,
            overscanX,
            overscanY);
    }

    private RevealLeafField.MotionSettings CreateMotionSettings()
    {
        return new RevealLeafField.MotionSettings(
            revealDuration,
            entryOffset,
            rotationFromSwipe,
            randomRotationJitter);
    }

    private void HandleInput(RevealLeafField.MotionSettings motionSettings)
    {
        if (backgroundRenderer == null || !leafField.HasLeaves)
            return;

        if (InputManager.TryGetPrimaryPointerUpThisFrame(out _))
        {
            isSwiping = false;
            return;
        }

        if (!InputManager.TryGetPrimaryPointerHeld(out Vector2 screenPosition))
        {
            isSwiping = false;
            return;
        }

        if (blockInputOverUI && InputManager.IsPrimaryPointerOverUI())
        {
            isSwiping = false;
            return;
        }

        if (!TryGetWorldPoint(screenPosition, out Vector2 worldPosition))
            return;

        if (!IsInsideRevealArea(worldPosition))
        {
            isSwiping = false;
            return;
        }

        Vector2 clampedWorldPosition = ClampToRevealArea(worldPosition);

        if (!isSwiping)
        {
            isSwiping = true;
            lastPointerWorld = clampedWorldPosition;
            lastSampleWorld = clampedWorldPosition;
            leafField.RevealAtPoint(clampedWorldPosition, Vector2.zero, brushRadius, motionSettings);
            return;
        }

        Vector2 delta = clampedWorldPosition - lastPointerWorld;
        if (delta.sqrMagnitude < minSwipeDistance * minSwipeDistance)
            return;

        Vector2 swipeDirection = delta.normalized;
        SampleBetween(lastSampleWorld, clampedWorldPosition, swipeDirection, motionSettings);
        lastPointerWorld = clampedWorldPosition;
    }

    private void SampleBetween(Vector2 from, Vector2 to, Vector2 swipeDirection, RevealLeafField.MotionSettings motionSettings)
    {
        if (sampleSpacing <= 0f)
        {
            leafField.RevealAtPoint(to, swipeDirection, brushRadius, motionSettings);
            lastSampleWorld = to;
            return;
        }

        float distance = Vector2.Distance(from, to);
        int stepCount = Mathf.Max(1, Mathf.CeilToInt(distance / sampleSpacing));

        for (int i = 1; i <= stepCount; i++)
        {
            float t = i / (float)stepCount;
            Vector2 point = Vector2.Lerp(from, to, t);
            leafField.RevealAtPoint(point, swipeDirection, brushRadius, motionSettings);
        }

        lastSampleWorld = to;
    }

    private bool TryGetWorldPoint(Vector2 screenPosition, out Vector2 worldPosition)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            worldPosition = Vector2.zero;
            return false;
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition = new Vector2(world.x, world.y);
        return true;
    }

    private bool IsInsideRevealArea(Vector2 worldPosition)
    {
        Bounds bounds = backgroundRenderer.bounds;
        return bounds.Contains(new Vector3(worldPosition.x, worldPosition.y, bounds.center.z));
    }

    private Vector2 ClampToRevealArea(Vector2 worldPosition)
    {
        Bounds bounds = backgroundRenderer.bounds;
        return new Vector2(
            Mathf.Clamp(worldPosition.x, bounds.min.x, bounds.max.x),
            Mathf.Clamp(worldPosition.y, bounds.min.y, bounds.max.y));
    }

    public void ResetReveal()
    {
        ResolveReferences();

        isSwiping = false;
        lastPointerWorld = Vector2.zero;
        lastSampleWorld = Vector2.zero;

        leafField.Clear();

        bool rebuilt = gridBuilder.RebuildGrid(
            backgroundRenderer,
            leafRoot,
            leafPrefab,
            CreateLayoutSettings(),
            this);

        if (!rebuilt)
            return;

        leafField.CacheLeaves(leafRoot, leafMask);
        leafField.HideAllLeaves();
    }
}
