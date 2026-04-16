using UnityEngine;

public sealed class RevealController : MonoBehaviour
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

    private readonly RevealLeafField leafField = new RevealLeafField();

    private Camera mainCamera;
    private bool isSwiping;
    private Vector2 lastPointerWorld;
    private Vector2 lastSampleWorld;

    private void Awake()
    {
        mainCamera = Camera.main;
        ResetReveal();
    }

    private void Update()
    {
        HandleInput();
        leafField.UpdateAnimations(Time.deltaTime, revealDuration);

        if (InputManager.ResetPressedThisFrame())
            ResetReveal();
    }

    [ContextMenu("Reset Reveal")]
    public void ResetReveal()
    {
        ResetSwipeState();

        if (!ValidateSetup())
        {
            leafField.Clear();
            return;
        }

        RebuildGrid();
        leafField.CacheLeaves(leafRoot, leafMask);
        leafField.HideAllLeaves();
    }

    private void HandleInput()
    {
        if (backgroundRenderer == null || !leafField.HasLeaves || !TryResolveCamera(out Camera activeCamera))
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

        Vector2 worldPosition = ScreenToWorld(activeCamera, screenPosition);
        if (!IsInsideBackground(worldPosition))
        {
            isSwiping = false;
            return;
        }

        if (!isSwiping)
        {
            BeginSwipe(worldPosition);
            return;
        }

        Vector2 delta = worldPosition - lastPointerWorld;
        if (delta.sqrMagnitude < minSwipeDistance * minSwipeDistance)
            return;

        SampleBetween(lastSampleWorld, worldPosition, delta.normalized);
        lastPointerWorld = worldPosition;
    }

    private void BeginSwipe(Vector2 worldPosition)
    {
        isSwiping = true;
        lastPointerWorld = worldPosition;
        lastSampleWorld = worldPosition;

        leafField.RevealAtPoint(
            worldPosition,
            Vector2.zero,
            brushRadius,
            entryOffset,
            rotationFromSwipe,
            randomRotationJitter);
    }

    private void SampleBetween(Vector2 from, Vector2 to, Vector2 swipeDirection)
    {
        if (sampleSpacing <= 0f)
        {
            RevealAtPoint(to, swipeDirection);
            lastSampleWorld = to;
            return;
        }

        float distance = Vector2.Distance(from, to);
        int stepCount = Mathf.Max(1, Mathf.CeilToInt(distance / sampleSpacing));

        for (int i = 1; i <= stepCount; i++)
        {
            float t = i / (float)stepCount;
            Vector2 point = Vector2.Lerp(from, to, t);
            RevealAtPoint(point, swipeDirection);
        }

        lastSampleWorld = to;
    }

    private void RevealAtPoint(Vector2 point, Vector2 swipeDirection)
    {
        leafField.RevealAtPoint(
            point,
            swipeDirection,
            brushRadius,
            entryOffset,
            rotationFromSwipe,
            randomRotationJitter);
    }

    private bool ValidateSetup()
    {
        if (backgroundRenderer == null)
        {
            Debug.LogWarning("RevealController requires a background SpriteRenderer.", this);
            return false;
        }

        if (leafRoot == null)
        {
            Debug.LogWarning("RevealController requires a leafRoot transform.", this);
            return false;
        }

        if (leafPrefab == null)
        {
            Debug.LogWarning("RevealController requires a leafPrefab.", this);
            return false;
        }

        if (leafMask.value == 0)
        {
            Debug.LogWarning("RevealController requires a valid leafMask.", this);
            return false;
        }

        if (spawnSpacingX <= 0f || spawnSpacingY <= 0f)
        {
            Debug.LogWarning("RevealController requires positive spawn spacing values.", this);
            return false;
        }

        if (!leafPrefab.TryGetComponent(out RevealLeaf leaf))
        {
            Debug.LogWarning("leafPrefab must have a RevealLeaf component on its root.", this);
            return false;
        }

        if (!leaf.HasRequiredReferences)
        {
            Debug.LogWarning(
                "leafPrefab RevealLeaf must assign a SpriteRenderer and Collider2D through serialized fields.",
                this);
            return false;
        }

        return true;
    }

    private void RebuildGrid()
    {
        ClearSpawnedLeaves();

        Bounds bounds = backgroundRenderer.bounds;
        float minX = bounds.min.x - overscanX;
        float maxX = bounds.max.x + overscanX;
        float minY = bounds.min.y - overscanY;
        float maxY = bounds.max.y + overscanY;

        int rowIndex = 0;
        for (float y = minY; y <= maxY + 0.001f; y += spawnSpacingY)
        {
            float xOffset = staggerOddRows && (rowIndex & 1) == 1 ? rowOffsetX : 0f;

            for (float x = minX; x <= maxX + 0.001f; x += spawnSpacingX)
                SpawnLeafInstance(new Vector2(x + xOffset, y));

            rowIndex++;
        }
    }

    private void ClearSpawnedLeaves()
    {
        for (int i = leafRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = leafRoot.GetChild(i);
            child.gameObject.SetActive(false);
            child.SetParent(null);

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    private void SpawnLeafInstance(Vector2 worldPosition)
    {
        GameObject leafInstance = Instantiate(leafPrefab, leafRoot, false);
        leafInstance.name = leafPrefab.name;

        Transform instanceTransform = leafInstance.transform;
        Vector3 position = instanceTransform.position;
        position.x = worldPosition.x;
        position.y = worldPosition.y;
        instanceTransform.position = position;
    }

    private bool TryResolveCamera(out Camera activeCamera)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        activeCamera = mainCamera;
        return activeCamera != null;
    }

    private Vector2 ScreenToWorld(Camera activeCamera, Vector2 screenPosition)
    {
        Vector3 world = activeCamera.ScreenToWorldPoint(screenPosition);
        return new Vector2(world.x, world.y);
    }

    private bool IsInsideBackground(Vector2 worldPosition)
    {
        Bounds bounds = backgroundRenderer.bounds;
        return bounds.Contains(new Vector3(worldPosition.x, worldPosition.y, bounds.center.z));
    }

    private void ResetSwipeState()
    {
        isSwiping = false;
        lastPointerWorld = Vector2.zero;
        lastSampleWorld = Vector2.zero;
    }
}
