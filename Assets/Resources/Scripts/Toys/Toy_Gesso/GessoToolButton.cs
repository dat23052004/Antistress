using UnityEngine;

[DisallowMultipleComponent]
public sealed class GessoToolButton : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private GessoToolMode toolMode = GessoToolMode.Chalk;
    [SerializeField] private Color chalkColor = Color.white;
    [SerializeField] private GameObject toolPrefab;

    [Header("References")]
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private SpriteRenderer visualRenderer;

    [Header("Animation")]
    [SerializeField] private float liftHeight = 0.25f;
    [SerializeField] private float liftSpeed = 4f;
    [SerializeField] private Vector3 standbyOffset = new Vector3(0f, 0.45f, 0f);
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0.15f, 0f);

    private Vector3 restLocalPosition;
    private bool isSelected;
    private bool hasWarnedMissingCollider;

    public GessoToolMode ToolMode => toolMode;
    public Color ChalkColor => chalkColor;
    public GameObject ToolPrefab => toolPrefab;

    private void Awake()
    {
        AutoAssignReferences();
        restLocalPosition = transform.localPosition;
    }

    private void Reset()
    {
        AutoAssignReferences();
        restLocalPosition = transform.localPosition;
    }

    private void OnValidate()
    {
        AutoAssignReferences();

        if (!Application.isPlaying)
            restLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        float targetY = restLocalPosition.y + (isSelected ? liftHeight : 0f);
        Vector3 localPosition = transform.localPosition;
        localPosition.y = Mathf.MoveTowards(localPosition.y, targetY, liftSpeed * Time.deltaTime);
        transform.localPosition = localPosition;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        if (hitCollider == null)
        {
            WarnMissingCollider();
            return false;
        }

        return hitCollider.OverlapPoint(worldPoint);
    }

    public Vector3 GetStandbyWorldPosition()
    {
        return transform.TransformPoint(standbyOffset);
    }

    public Vector3 GetFollowWorldPosition(Vector2 pointerWorld)
    {
        Vector3 followPosition = new Vector3(pointerWorld.x, pointerWorld.y, transform.position.z);
        return followPosition + followOffset;
    }

    public void ApplyColorToInstance(GameObject instance)
    {
        if (toolMode != GessoToolMode.Chalk || instance == null)
            return;

        SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = chalkColor;
    }

    private void AutoAssignReferences()
    {
        if (hitCollider == null)
            hitCollider = GetComponent<Collider2D>();

        if (visualRenderer == null)
            visualRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void WarnMissingCollider()
    {
        if (hasWarnedMissingCollider)
            return;

        hasWarnedMissingCollider = true;
        Debug.LogWarning($"GessoToolButton on '{name}' requires a Collider2D reference.", this);
    }
}
