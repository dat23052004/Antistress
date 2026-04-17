using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GessoBoard : MonoBehaviour
{
    private const float MinSegmentLengthSqr = 0.000001f;

    [Header("References")]
    [SerializeField] private Collider2D boardCollider;
    [SerializeField] private SpriteRenderer boardRenderer;
    [SerializeField] private Transform strokeRoot;
    [SerializeField] private Material strokeMaterial;
    [SerializeField] private Texture2D strokeStampTexture;

    [Header("Stroke")]
    [SerializeField] private float sampleSpacing = 0.08f;
    [SerializeField] private float strokeWidth = 0.12f;
    [SerializeField] private float eraseRadius = 0.18f;

    [Header("Stamp Look")]
    [SerializeField] private float stampSpacing = 0.05f;
    [SerializeField] private float stampOffsetJitter = 0.02f;
    [SerializeField] private Vector2 stampScaleRange = new Vector2(0.85f, 1.15f);
    [SerializeField] private Vector2 stampAlphaRange = new Vector2(0.45f, 0.8f);
    [SerializeField] private float stampAngleJitter = 24f;

    [Header("Sorting")]
    [SerializeField] private string strokeSortingLayerName = "Default";
    [SerializeField] private int strokeSortingOrder = 1;

    private readonly List<GessoStrokeSegment> strokeSegments = new List<GessoStrokeSegment>();
    private bool hasWarnedInvalidSetup;

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void OnValidate()
    {
        AutoAssignReferences();
    }

    public bool ContainsPoint(Vector2 worldPoint)
    {
        if (boardCollider == null)
        {
            WarnInvalidSetup("GessoBoard requires a Collider2D assigned to boardCollider.");
            return false;
        }

        return boardCollider.OverlapPoint(worldPoint);
    }

    public void DrawBetween(Vector2 from, Vector2 to, Color color)
    {
        if (!HasRequiredSetup())
            return;

        SamplePath(
            from,
            to,
            CreateStrokeSegment,
            color);
    }

    public void EraseBetween(Vector2 from, Vector2 to)
    {
        if (!HasRequiredSetup())
            return;

        SamplePath(
            from,
            to,
            EraseAtPoint,
            default);
    }

    public void ClearAllStrokes()
    {
        for (int i = strokeSegments.Count - 1; i >= 0; i--)
        {
            if (strokeSegments[i] == null)
                continue;

            DestroySegment(strokeSegments[i].gameObject);
        }

        strokeSegments.Clear();
    }

    private void SamplePath(
        Vector2 from,
        Vector2 to,
        System.Action<Vector2, Vector2, Color> segmentAction,
        Color color)
    {
        float distance = Vector2.Distance(from, to);
        int stepCount = sampleSpacing <= 0f
            ? 1
            : Mathf.Max(1, Mathf.CeilToInt(distance / sampleSpacing));

        Vector2 previousPoint = from;
        for (int i = 1; i <= stepCount; i++)
        {
            float t = i / (float)stepCount;
            Vector2 currentPoint = Vector2.Lerp(from, to, t);
            segmentAction(previousPoint, currentPoint, color);
            previousPoint = currentPoint;
        }
    }

    private void CreateStrokeSegment(Vector2 from, Vector2 to, Color color)
    {
        if ((to - from).sqrMagnitude <= MinSegmentLengthSqr)
            return;

        GameObject segmentObject = new GameObject("GessoStrokeSegment");
        segmentObject.transform.SetParent(strokeRoot, false);

        GessoStrokeSegment segment = segmentObject.AddComponent<GessoStrokeSegment>();
        segment.Initialize(
            from,
            to,
            strokeWidth,
            color,
            strokeStampTexture,
            strokeMaterial,
            strokeSortingLayerName,
            strokeSortingOrder,
            stampSpacing,
            stampOffsetJitter,
            stampScaleRange,
            stampAlphaRange,
            stampAngleJitter);

        strokeSegments.Add(segment);
    }

    private void EraseAtPoint(Vector2 from, Vector2 to, Color _)
    {
        Vector2 samplePoint = to;
        float radius = Mathf.Max(eraseRadius, 0.0001f);

        for (int i = strokeSegments.Count - 1; i >= 0; i--)
        {
            GessoStrokeSegment segment = strokeSegments[i];
            if (segment == null)
            {
                strokeSegments.RemoveAt(i);
                continue;
            }

            if (segment.DistanceTo(samplePoint) > radius)
                continue;

            DestroySegment(segment.gameObject);
            strokeSegments.RemoveAt(i);
        }
    }

    private bool HasRequiredSetup()
    {
        if (boardCollider == null)
        {
            WarnInvalidSetup("GessoBoard requires a Collider2D assigned to boardCollider.");
            return false;
        }

        if (boardRenderer == null)
        {
            WarnInvalidSetup("GessoBoard requires a SpriteRenderer assigned to boardRenderer.");
            return false;
        }

        if (strokeRoot == null)
        {
            WarnInvalidSetup("GessoBoard requires a Transform assigned to strokeRoot.");
            return false;
        }

        if (strokeWidth <= 0f)
        {
            WarnInvalidSetup("GessoBoard requires strokeWidth to be greater than zero.");
            return false;
        }

        if (strokeStampTexture == null)
        {
            WarnInvalidSetup("GessoBoard requires a Texture2D assigned to strokeStampTexture.");
            return false;
        }

        return true;
    }

    private void AutoAssignReferences()
    {
        if (boardCollider == null)
            boardCollider = GetComponent<Collider2D>();

        if (boardRenderer == null)
            boardRenderer = GetComponent<SpriteRenderer>();
    }

    private void WarnInvalidSetup(string message)
    {
        if (hasWarnedInvalidSetup)
            return;

        hasWarnedInvalidSetup = true;
        Debug.LogWarning(message, this);
    }

    private static void DestroySegment(GameObject segmentObject)
    {
        if (segmentObject == null)
            return;

        if (Application.isPlaying)
            Destroy(segmentObject);
        else
            DestroyImmediate(segmentObject);
    }
}
