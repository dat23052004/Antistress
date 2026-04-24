using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public sealed class GessoBoard : MonoBehaviour
{
    private const float MinSegmentLengthSqr = 0.000001f;

    [Header("References")]
    [SerializeField] private Collider2D boardCollider;
    [SerializeField] private Transform strokeRoot;

    [Header("Stroke")]
    [SerializeField] private float sampleSpacing = 0.08f;
    [SerializeField] private float strokeWidth = 0.12f;
    [SerializeField] private float eraseRadius = 0.18f;

    [Header("Sorting")]
    [SerializeField] private string strokeSortingLayerName = "Default";
    [SerializeField] private int strokeSortingOrder = 1;

    [Header("Pooling")]
    [SerializeField] private GameObject stampPrefab;
    [SerializeField] private int poolDefaultCapacity = 256;
    [SerializeField] private int poolMaxSize = 4096;

    private readonly List<GessoStrokeSegment> strokeSegments = new List<GessoStrokeSegment>();
    private ObjectPool<SpriteRenderer> stampPool;
    private Transform stampPoolHolder;
    private float stampSpriteWorldWidth = 1f;
    private bool hasWarnedSetup;

    internal float StampSpriteWorldWidth => stampSpriteWorldWidth;

    private void Awake() => InitializeStampPool();
    private void OnDestroy() => stampPool?.Dispose();
    private void Reset() => AutoAssignReferences();
    private void OnValidate() => AutoAssignReferences();

    public bool ContainsPoint(Vector2 worldPoint) =>
        boardCollider != null && boardCollider.OverlapPoint(worldPoint);

    internal void DrawBetween(GessoBrushPoint from, GessoBrushPoint to, Color color)
    {
        if (!HasRequiredSetup()) return;

        GessoStrokeStyle style = new(strokeWidth, color, strokeSortingLayerName, strokeSortingOrder);
        int steps = GetStepCount(Vector2.Distance(from.worldPosition, to.worldPosition));
        GessoBrushPoint prev = from;
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            GessoBrushPoint cur = LerpBrushPoint(from, to, t);
            AddStrokeSegment(prev, cur, style);
            prev = cur;
        }
    }

    public void EraseBetween(Vector2 from, Vector2 to)
    {
        if (!HasRequiredSetup()) return;

        int steps = GetStepCount(Vector2.Distance(from, to));
        float radius = Mathf.Max(eraseRadius, 0.0001f);
        for (int i = 1; i <= steps; i++)
        {
            Vector2 point = Vector2.Lerp(from, to, i / (float)steps);
            for (int j = strokeSegments.Count - 1; j >= 0; j--)
            {
                if (strokeSegments[j] == null) { strokeSegments.RemoveAt(j); continue; }
                if (strokeSegments[j].DistanceTo(point) <= radius)
                {
                    DestroySegment(strokeSegments[j]);
                    strokeSegments.RemoveAt(j);
                }
            }
        }
    }

    public void ClearAllStrokes()
    {
        for (int i = strokeSegments.Count - 1; i >= 0; i--)
            if (strokeSegments[i] != null) DestroySegment(strokeSegments[i]);
        strokeSegments.Clear();
    }

    internal SpriteRenderer AcquireStamp(Transform parent)
    {
        if (stampPool == null) InitializeStampPool();
        SpriteRenderer stamp = stampPool.Get();
        if (stamp != null && parent != null) stamp.transform.SetParent(parent, false);
        return stamp;
    }

    internal void ReleaseStamp(SpriteRenderer stamp)
    {
        if (stamp != null && stampPool != null) stampPool.Release(stamp);
    }

    private void InitializeStampPool()
    {
        if (stampPool != null) return;

        if (stampPoolHolder == null)
        {
            GameObject holder = new("GessoStampPool");
            holder.transform.SetParent(transform, false);
            holder.SetActive(false);
            stampPoolHolder = holder.transform;
        }

        if (stampPrefab != null &&
            stampPrefab.TryGetComponent(out SpriteRenderer prefabSr) &&
            prefabSr.sprite != null)
        {
            stampSpriteWorldWidth = Mathf.Max(prefabSr.sprite.bounds.size.x, 0.0001f);
        }

        stampPool = new ObjectPool<SpriteRenderer>(
            createFunc: CreateStamp,
            actionOnGet: sr => { if (sr) sr.gameObject.SetActive(true); },
            actionOnRelease: sr =>
            {
                if (!sr) return;
                sr.transform.SetParent(stampPoolHolder, false);
                sr.gameObject.SetActive(false);
            },
            actionOnDestroy: sr =>
            {
                if (!sr) return;
                if (Application.isPlaying) Destroy(sr.gameObject);
                else DestroyImmediate(sr.gameObject);
            },
            defaultCapacity: Mathf.Max(0, poolDefaultCapacity),
            maxSize: Mathf.Max(1, poolMaxSize));
    }

    private SpriteRenderer CreateStamp()
    {
        GameObject go;
        if (stampPrefab != null)
        {
            go = Instantiate(stampPrefab, stampPoolHolder);
        }
        else
        {
            go = new GameObject("Stamp");
            go.transform.SetParent(stampPoolHolder, false);
        }

        if (!go.TryGetComponent(out SpriteRenderer sr))
            sr = go.AddComponent<SpriteRenderer>();

        go.SetActive(false);
        return sr;
    }

    private void AddStrokeSegment(GessoBrushPoint from, GessoBrushPoint to, GessoStrokeStyle style)
    {
        if ((to.worldPosition - from.worldPosition).sqrMagnitude <= MinSegmentLengthSqr) return;

        GameObject go = new("GessoStrokeSegment");
        go.transform.SetParent(strokeRoot, false);
        GessoStrokeSegment seg = go.AddComponent<GessoStrokeSegment>();
        seg.Initialize(this, from, to, style);
        strokeSegments.Add(seg);
    }

    private void DestroySegment(GessoStrokeSegment seg)
    {
        seg.ReleaseStamps();
        if (Application.isPlaying) Destroy(seg.gameObject);
        else DestroyImmediate(seg.gameObject);
    }

    private int GetStepCount(float distance) =>
        sampleSpacing > 0f ? Mathf.Max(1, Mathf.CeilToInt(distance / sampleSpacing)) : 1;

    private static GessoBrushPoint LerpBrushPoint(GessoBrushPoint a, GessoBrushPoint b, float t) =>
        new(Vector2.Lerp(a.worldPosition, b.worldPosition, t),
            Mathf.Lerp(a.strokeDistance, b.strokeDistance, t),
            Mathf.Lerp(a.speed, b.speed, t),
            Mathf.Lerp(a.alpha01, b.alpha01, t));

    private bool HasRequiredSetup()
    {
        if (boardCollider != null && strokeRoot != null && stampPrefab != null && strokeWidth > 0f)
            return true;
        if (!hasWarnedSetup)
        {
            hasWarnedSetup = true;
            Debug.LogWarning("GessoBoard: requires boardCollider, strokeRoot, stampPrefab, and strokeWidth > 0.", this);
        }
        return false;
    }

    private void AutoAssignReferences()
    {
        if (boardCollider == null)
            boardCollider = GetComponent<Collider2D>();
    }
}
