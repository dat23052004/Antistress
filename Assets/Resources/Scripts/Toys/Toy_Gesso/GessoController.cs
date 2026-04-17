using UnityEngine;

[DisallowMultipleComponent]
public sealed class GessoController : MonoBehaviour
{
    private const int HeldToolSortingOrder = 10;

    [Header("References")]
    [SerializeField] private GessoBoard board;
    [SerializeField] private Transform heldToolRoot;
    [SerializeField] private GessoToolButton[] toolButtons;

    [Header("Input")]
    [SerializeField] private bool blockInputOverUI = true;

    private Camera mainCamera;
    private GessoToolButton selectedTool;
    private GameObject heldToolInstance;
    private bool isBoardSession;
    private bool wasPointerInsideBoardLastFrame;
    private Vector2 lastBoardPoint;
    private bool hasWarnedMissingSetup;

    private void Awake()
    {
        AutoAssignReferences();
        mainCamera = Camera.main;
        ResetSelectionState();
    }

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void OnValidate()
    {
        AutoAssignReferences();
    }

    private void Update()
    {
        HandleInput();

        if (!isBoardSession)
            MoveHeldToolToStandby();
    }

    private void HandleInput()
    {
        if (InputManager.TryGetPrimaryPointerUpThisFrame(out _))
            EndBoardSession();

        if (!TryResolveCamera(out Camera activeCamera))
            return;

        if (blockInputOverUI && InputManager.IsPrimaryPointerOverUI())
        {
            EndBoardSession();
            return;
        }

        if (InputManager.TryGetPrimaryPointerDownThisFrame(out Vector2 downScreenPosition))
            HandlePointerDown(activeCamera, downScreenPosition);

        if (InputManager.TryGetPrimaryPointerHeld(out Vector2 heldScreenPosition))
            HandlePointerHeld(activeCamera, heldScreenPosition);
    }

    private void HandlePointerDown(Camera activeCamera, Vector2 screenPosition)
    {
        Vector2 worldPoint = ScreenToWorld(activeCamera, screenPosition);

        GessoToolButton hitTool = FindToolAt(worldPoint);
        if (hitTool != null)
        {
            SelectTool(hitTool);
            EndBoardSession();
            return;
        }

        if (selectedTool == null || board == null)
            return;

        if (!board.ContainsPoint(worldPoint))
            return;

        isBoardSession = true;
        wasPointerInsideBoardLastFrame = true;
        lastBoardPoint = worldPoint;

        MoveHeldToolToPointer(worldPoint);

        if (selectedTool.ToolMode == GessoToolMode.Eraser)
            board.EraseBetween(worldPoint, worldPoint);
    }

    private void HandlePointerHeld(Camera activeCamera, Vector2 screenPosition)
    {
        Vector2 worldPoint = ScreenToWorld(activeCamera, screenPosition);

        if (!isBoardSession)
            return;

        MoveHeldToolToPointer(worldPoint);

        if (board == null)
        {
            WarnMissingSetup("GessoController requires a GessoBoard reference.");
            EndBoardSession();
            return;
        }

        bool isInsideBoard = board.ContainsPoint(worldPoint);
        if (!isInsideBoard)
        {
            wasPointerInsideBoardLastFrame = false;
            return;
        }

        if (!wasPointerInsideBoardLastFrame)
        {
            lastBoardPoint = worldPoint;
            wasPointerInsideBoardLastFrame = true;
            return;
        }

        if (selectedTool.ToolMode == GessoToolMode.Chalk)
            board.DrawBetween(lastBoardPoint, worldPoint, selectedTool.ChalkColor);
        else
            board.EraseBetween(lastBoardPoint, worldPoint);

        lastBoardPoint = worldPoint;
    }

    private void SelectTool(GessoToolButton tool)
    {
        if (tool == null)
            return;

        if (selectedTool == tool)
        {
            EnsureHeldToolInstance();
            tool.SetSelected(true);
            MoveHeldToolToStandby();
            return;
        }

        if (selectedTool != null)
            selectedTool.SetSelected(false);

        selectedTool = tool;
        selectedTool.SetSelected(true);

        RebuildHeldToolInstance();
        MoveHeldToolToStandby();
    }

    private void EnsureHeldToolInstance()
    {
        if (selectedTool == null || heldToolInstance != null)
            return;

        RebuildHeldToolInstance();
    }

    private void RebuildHeldToolInstance()
    {
        DestroyHeldToolInstance();

        if (selectedTool == null)
            return;

        if (selectedTool.ToolPrefab == null)
        {
            Debug.LogWarning($"GessoToolButton '{selectedTool.name}' is missing a toolPrefab.", selectedTool);
            return;
        }

        Transform parent = heldToolRoot != null ? heldToolRoot : transform;
        heldToolInstance = Instantiate(selectedTool.ToolPrefab, parent, false);
        heldToolInstance.name = $"{selectedTool.ToolPrefab.name}_Held";

        DisableHeldToolColliders();
        SetHeldToolSortingOrder();
        selectedTool.ApplyColorToInstance(heldToolInstance);
    }

    private void DisableHeldToolColliders()
    {
        if (heldToolInstance == null)
            return;

        Collider2D[] colliders = heldToolInstance.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;
    }

    private void SetHeldToolSortingOrder()
    {
        if (heldToolInstance == null)
            return;

        SpriteRenderer[] renderers = heldToolInstance.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingLayerName = "Default";
            renderers[i].sortingOrder = HeldToolSortingOrder;
        }
    }

    private void MoveHeldToolToStandby()
    {
        if (selectedTool == null || heldToolInstance == null)
            return;

        heldToolInstance.transform.position = selectedTool.GetStandbyWorldPosition();
    }

    private void MoveHeldToolToPointer(Vector2 worldPoint)
    {
        if (selectedTool == null || heldToolInstance == null)
            return;

        heldToolInstance.transform.position = selectedTool.GetFollowWorldPosition(worldPoint);
    }

    private GessoToolButton FindToolAt(Vector2 worldPoint)
    {
        if (toolButtons == null)
            return null;

        for (int i = 0; i < toolButtons.Length; i++)
        {
            GessoToolButton toolButton = toolButtons[i];
            if (toolButton == null)
                continue;

            if (toolButton.ContainsWorldPoint(worldPoint))
                return toolButton;
        }

        return null;
    }

    private void EndBoardSession()
    {
        isBoardSession = false;
        wasPointerInsideBoardLastFrame = false;
        lastBoardPoint = Vector2.zero;
    }

    private void ResetSelectionState()
    {
        selectedTool = null;
        EndBoardSession();

        if (toolButtons == null)
            return;

        for (int i = 0; i < toolButtons.Length; i++)
        {
            if (toolButtons[i] != null)
                toolButtons[i].SetSelected(false);
        }
    }

    private void DestroyHeldToolInstance()
    {
        if (heldToolInstance == null)
            return;

        if (Application.isPlaying)
            Destroy(heldToolInstance);
        else
            DestroyImmediate(heldToolInstance);

        heldToolInstance = null;
    }

    private void AutoAssignReferences()
    {
        if (board == null)
            board = GetComponentInChildren<GessoBoard>(true);

        if (heldToolRoot == null)
        {
            Transform candidate = transform.Find("HeldToolRoot");
            heldToolRoot = candidate != null ? candidate : transform;
        }

        if (toolButtons == null || toolButtons.Length == 0)
            toolButtons = GetComponentsInChildren<GessoToolButton>(true);
    }

    private bool TryResolveCamera(out Camera activeCamera)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        activeCamera = mainCamera;
        if (activeCamera != null)
            return true;

        WarnMissingSetup("GessoController could not find Camera.main.");
        return false;
    }

    private void WarnMissingSetup(string message)
    {
        if (hasWarnedMissingSetup)
            return;

        hasWarnedMissingSetup = true;
        Debug.LogWarning(message, this);
    }

    private static Vector2 ScreenToWorld(Camera activeCamera, Vector2 screenPosition)
    {
        Vector3 screenPoint = new Vector3(
            screenPosition.x,
            screenPosition.y,
            -activeCamera.transform.position.z);

        Vector3 worldPoint = activeCamera.ScreenToWorldPoint(screenPoint);
        return new Vector2(worldPoint.x, worldPoint.y);
    }
}
