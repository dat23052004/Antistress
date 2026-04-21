using UnityEngine;

[DisallowMultipleComponent]
public sealed class GessoController : MonoBehaviour
{
    private const int HeldToolSortingOrder = 10;
    private const float MinPointDistanceSqr = 0.0009f;

    [Header("References")]
    [SerializeField] private GessoBoard board;
    [SerializeField] private Transform heldToolRoot;
    [SerializeField] private GessoToolButton[] toolButtons;

    [Header("Input")]
    [SerializeField] private bool blockInputOverUI = true;

    [Header("Taper")]
    [SerializeField] private float startTaperLength = 0.08f;
    [SerializeField] private float endTaperLength = 0.12f;
    [SerializeField] private float minimumTailLength = 0.04f;
    [SerializeField] private float syntheticTailLength = 0.08f;

    private Camera mainCamera;
    private GessoToolButton selectedTool;
    private GameObject heldToolInstance;
    private bool isBoardSession;
    private bool wasPointerInsideBoardLastFrame;
    private Vector2 lastBoardPoint;
    private GessoBrushPoint lastBrushPoint;
    private bool hasLastBrushPoint;
    private float cumulativeStrokeDistance;
    private Vector2 lastMovementDirection;
    private bool hasLastMovementDirection;
    private bool hasHeldPosition;
    private bool hasWarnedSetup;

    private void Awake()
    {
        AutoAssignReferences();
        mainCamera = Camera.main;
        ResetSelectionState();
    }

    private void Reset() => AutoAssignReferences();
    private void OnValidate() => AutoAssignReferences();

    private void Update()
    {
        HandleInput();
        if (!isBoardSession && !hasHeldPosition) MoveHeldToolToStandby();
    }

    private void HandleInput()
    {
        if (!TryResolveCamera(out Camera cam)) return;
        if (blockInputOverUI && InputManager.IsPrimaryPointerOverUI()) { EndBoardSession(); return; }

        if (InputManager.TryGetPrimaryPointerDownDetailed(out PrimaryPointerSample down))
            HandlePointerDown(cam, down);
        if (InputManager.TryGetPrimaryPointerHeldDetailed(out PrimaryPointerSample held))
            HandlePointerHeld(cam, held);
        if (InputManager.TryGetPrimaryPointerUpDetailed(out PrimaryPointerSample up))
            HandlePointerUp(cam, up);
    }

    private void HandlePointerDown(Camera cam, PrimaryPointerSample sample)
    {
        Vector2 worldPoint = ScreenToWorld(cam, sample.screenPosition);

        GessoToolButton hitTool = FindToolAt(worldPoint);
        if (hitTool != null) { SelectTool(hitTool); EndBoardSession(); return; }

        if (selectedTool == null || board == null || !board.ContainsPoint(worldPoint)) return;

        isBoardSession = true;
        wasPointerInsideBoardLastFrame = true;
        cumulativeStrokeDistance = 0f;
        hasLastMovementDirection = false;
        lastBoardPoint = worldPoint;
        MoveHeldToolToPointer(worldPoint);

        if (selectedTool.ToolMode == GessoToolMode.Eraser)
        {
            board.EraseBetween(worldPoint, worldPoint);
            hasLastBrushPoint = false;
            return;
        }

        lastBrushPoint = MakeBrushPoint(worldPoint, 0f, 0f);
        hasLastBrushPoint = true;
    }

    private void HandlePointerHeld(Camera cam, PrimaryPointerSample sample)
    {
        if (!isBoardSession) return;

        Vector2 worldPoint = ScreenToWorld(cam, sample.screenPosition);
        MoveHeldToolToPointer(worldPoint);

        if (board == null || selectedTool == null) { EndBoardSession(); return; }
        if (!board.ContainsPoint(worldPoint)) { wasPointerInsideBoardLastFrame = false; return; }

        if (!wasPointerInsideBoardLastFrame)
        {
            wasPointerInsideBoardLastFrame = true;
            lastBoardPoint = worldPoint;
            if (selectedTool.ToolMode == GessoToolMode.Chalk)
            {
                lastBrushPoint = MakeBrushPoint(worldPoint, 0f, cumulativeStrokeDistance);
                hasLastBrushPoint = true;
            }
            return;
        }

        if (selectedTool.ToolMode == GessoToolMode.Eraser)
        {
            board.EraseBetween(lastBoardPoint, worldPoint);
            lastBoardPoint = worldPoint;
            return;
        }

        if (!hasLastBrushPoint)
        {
            lastBrushPoint = MakeBrushPoint(worldPoint, 0f, cumulativeStrokeDistance);
            hasLastBrushPoint = true;
            lastBoardPoint = worldPoint;
            return;
        }

        Vector2 delta = worldPoint - lastBrushPoint.worldPosition;
        if (delta.sqrMagnitude <= MinPointDistanceSqr) return;

        float dist = delta.magnitude;
        float speed = dist / Mathf.Max(Time.deltaTime, 0.001f);
        cumulativeStrokeDistance += dist;

        GessoBrushPoint current = MakeBrushPoint(worldPoint, speed, cumulativeStrokeDistance);
        board.DrawBetween(lastBrushPoint, current, selectedTool.ChalkColor);

        lastMovementDirection = delta.normalized;
        hasLastMovementDirection = true;
        lastBrushPoint = current;
        lastBoardPoint = worldPoint;
    }

    private void HandlePointerUp(Camera cam, PrimaryPointerSample sample)
    {
        if (isBoardSession && selectedTool != null &&
            selectedTool.ToolMode == GessoToolMode.Chalk &&
            board != null && hasLastBrushPoint)
        {
            DrawReleaseTail(cam, sample);
        }

        EndBoardSession();
    }

    private void DrawReleaseTail(Camera cam, PrimaryPointerSample sample)
    {
        Vector2 liftPoint = ScreenToWorld(cam, sample.screenPosition);
        Vector2 dir = ResolveTailDirection(liftPoint);
        if (dir == Vector2.zero) return;

        float releaseDistance = Vector2.Distance(lastBrushPoint.worldPosition, liftPoint);
        bool useActual = board.ContainsPoint(liftPoint) && releaseDistance >= minimumTailLength;

        float tailLength = useActual
            ? Mathf.Max(Mathf.Min(releaseDistance, endTaperLength), minimumTailLength)
            : Mathf.Max(syntheticTailLength, minimumTailLength);

        Vector2 desiredEnd = lastBrushPoint.worldPosition + dir * tailLength;
        Vector2 tailEnd = ClampTailEndpointToBoard(lastBrushPoint.worldPosition, desiredEnd);

        if ((tailEnd - lastBrushPoint.worldPosition).sqrMagnitude <= MinPointDistanceSqr) return;

        cumulativeStrokeDistance += Vector2.Distance(lastBrushPoint.worldPosition, tailEnd);
        GessoBrushPoint tailPoint = new(tailEnd, cumulativeStrokeDistance, 0f, 0f);
        board.DrawBetween(lastBrushPoint, tailPoint, selectedTool.ChalkColor);
    }

    private GessoBrushPoint MakeBrushPoint(Vector2 position, float speed, float strokeDistance)
    {
        float alpha = startTaperLength > 0f ? Mathf.Clamp01(strokeDistance / startTaperLength) : 1f;
        return new GessoBrushPoint(position, strokeDistance, speed, alpha);
    }

    private Vector2 ResolveTailDirection(Vector2 liftPoint)
    {
        Vector2 delta = liftPoint - lastBrushPoint.worldPosition;
        if (delta.sqrMagnitude > MinPointDistanceSqr) return delta.normalized;
        if (hasLastMovementDirection && lastMovementDirection.sqrMagnitude > MinPointDistanceSqr)
            return lastMovementDirection;
        return Vector2.zero;
    }

    private Vector2 ClampTailEndpointToBoard(Vector2 start, Vector2 desiredEnd)
    {
        if (board == null || board.ContainsPoint(desiredEnd)) return desiredEnd;
        Vector2 delta = desiredEnd - start;
        for (int i = 0; i < 5; i++)
        {
            delta *= 0.5f;
            Vector2 candidate = start + delta;
            if (board.ContainsPoint(candidate)) return candidate;
        }
        return start;
    }

    private void SelectTool(GessoToolButton tool)
    {
        if (tool == null) return;
        if (selectedTool == tool) { EnsureHeldToolInstance(); tool.SetSelected(true); MoveHeldToolToStandby(); return; }
        if (selectedTool != null) selectedTool.SetSelected(false);
        selectedTool = tool;
        selectedTool.SetSelected(true);
        RebuildHeldToolInstance();
        MoveHeldToolToStandby();
    }

    private void EnsureHeldToolInstance()
    {
        if (selectedTool != null && heldToolInstance == null)
            RebuildHeldToolInstance();
    }

    private void RebuildHeldToolInstance()
    {
        DestroyHeldToolInstance();
        hasHeldPosition = false;

        if (selectedTool == null || selectedTool.ToolPrefab == null)
        {
            if (selectedTool != null)
                Debug.LogWarning($"GessoToolButton '{selectedTool.name}' is missing a toolPrefab.", selectedTool);
            return;
        }

        Transform parent = heldToolRoot != null ? heldToolRoot : transform;
        heldToolInstance = Instantiate(selectedTool.ToolPrefab, parent, false);
        heldToolInstance.name = $"{selectedTool.ToolPrefab.name}_Held";

        foreach (Collider2D col in heldToolInstance.GetComponentsInChildren<Collider2D>(true))
            col.enabled = false;

        foreach (SpriteRenderer sr in heldToolInstance.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = HeldToolSortingOrder;
        }

        selectedTool.ApplyColorToInstance(heldToolInstance);
    }

    private void MoveHeldToolToStandby()
    {
        if (selectedTool != null && heldToolInstance != null)
            heldToolInstance.transform.position = selectedTool.GetStandbyWorldPosition();
    }

    private void MoveHeldToolToPointer(Vector2 worldPoint)
    {
        if (selectedTool == null || heldToolInstance == null) return;
        heldToolInstance.transform.position = selectedTool.GetFollowWorldPosition(worldPoint);
        hasHeldPosition = true;
    }

    private GessoToolButton FindToolAt(Vector2 worldPoint)
    {
        if (toolButtons == null) return null;
        for (int i = 0; i < toolButtons.Length; i++)
            if (toolButtons[i] != null && toolButtons[i].ContainsWorldPoint(worldPoint))
                return toolButtons[i];
        return null;
    }

    private void EndBoardSession()
    {
        isBoardSession = false;
        wasPointerInsideBoardLastFrame = false;
        lastBoardPoint = Vector2.zero;
        hasLastBrushPoint = false;
        cumulativeStrokeDistance = 0f;
        lastMovementDirection = Vector2.zero;
        hasLastMovementDirection = false;
    }

    private void ResetSelectionState()
    {
        selectedTool = null;
        EndBoardSession();
        if (toolButtons == null) return;
        for (int i = 0; i < toolButtons.Length; i++)
            if (toolButtons[i] != null) toolButtons[i].SetSelected(false);
    }

    private void DestroyHeldToolInstance()
    {
        if (heldToolInstance == null) return;
        if (Application.isPlaying) Destroy(heldToolInstance);
        else DestroyImmediate(heldToolInstance);
        heldToolInstance = null;
    }

    private void AutoAssignReferences()
    {
        if (board == null) board = GetComponentInChildren<GessoBoard>(true);
        if (heldToolRoot == null)
        {
            Transform candidate = transform.Find("HeldToolRoot");
            heldToolRoot = candidate != null ? candidate : transform;
        }
        if (toolButtons == null || toolButtons.Length == 0)
            toolButtons = GetComponentsInChildren<GessoToolButton>(true);
    }

    private bool TryResolveCamera(out Camera cam)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        cam = mainCamera;
        if (cam != null) return true;
        if (!hasWarnedSetup) { hasWarnedSetup = true; Debug.LogWarning("GessoController: Camera.main not found.", this); }
        return false;
    }

    private static Vector2 ScreenToWorld(Camera cam, Vector2 screenPos) =>
        cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
}
