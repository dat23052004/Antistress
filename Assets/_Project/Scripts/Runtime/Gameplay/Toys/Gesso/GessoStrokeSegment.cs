using System.Collections.Generic;
using UnityEngine;

internal readonly struct GessoStrokeStyle
{
    public readonly float baseStrokeWidth;
    public readonly Color color;
    public readonly string sortingLayerName;
    public readonly int sortingOrder;

    public GessoStrokeStyle(float baseStrokeWidth, Color color, string sortingLayerName, int sortingOrder)
    {
        this.baseStrokeWidth = baseStrokeWidth;
        this.color = color;
        this.sortingLayerName = sortingLayerName;
        this.sortingOrder = sortingOrder;
    }
}

[DisallowMultipleComponent]
public sealed class GessoStrokeSegment : MonoBehaviour
{
    private const float MinSegmentLengthSqr = 0.000001f;

    private Vector2 start;
    private Vector2 end;
    private GessoBoard board;
    private readonly List<SpriteRenderer> stamps = new List<SpriteRenderer>();

    internal void Initialize(GessoBoard board, GessoBrushPoint from, GessoBrushPoint to, GessoStrokeStyle style)
    {
        this.board = board;
        start = from.worldPosition;
        end = to.worldPosition;

        if (board == null) return;

        float distance = Vector2.Distance(start, end);
        float scale = style.baseStrokeWidth / board.StampSpriteWorldWidth;
        float spacing = Mathf.Clamp(style.baseStrokeWidth * 0.4f, 0.02f, 0.06f);

        bool drewFinalStamp = false;
        float d = 0f;
        while (d <= distance + 0.0001f)
        {
            float t = distance > 0.0001f ? Mathf.Clamp01(d / distance) : 0f;
            PlaceStamp(Vector2.Lerp(from.worldPosition, to.worldPosition, t),
                scale, Mathf.Lerp(from.alpha01, to.alpha01, t), style);
            d += spacing;
            drewFinalStamp = t >= 0.999f;
        }

        if (!drewFinalStamp)
            PlaceStamp(to.worldPosition, scale, to.alpha01, style);
    }

    public float DistanceTo(Vector2 point)
    {
        Vector2 seg = end - start;
        float lenSqr = seg.sqrMagnitude;
        if (lenSqr <= MinSegmentLengthSqr)
            return Vector2.Distance(point, start);
        float t = Mathf.Clamp01(Vector2.Dot(point - start, seg) / lenSqr);
        return Vector2.Distance(point, start + seg * t);
    }

    private void PlaceStamp(Vector2 position, float scale, float alpha, GessoStrokeStyle style)
    {
        if (alpha <= 0.0001f) return;

        SpriteRenderer sr = board.AcquireStamp(transform);
        if (sr == null) return;

        sr.transform.SetPositionAndRotation(
            new Vector3(position.x, position.y, transform.position.z), Quaternion.identity);
        sr.transform.localScale = new Vector3(scale, scale, 1f);
        sr.sortingLayerName = style.sortingLayerName;
        sr.sortingOrder = style.sortingOrder;

        Color c = style.color;
        c.a *= Mathf.Clamp01(alpha);
        sr.color = c;

        stamps.Add(sr);
    }

    internal void ReleaseStamps()
    {
        if (board != null)
            for (int i = 0; i < stamps.Count; i++)
                board.ReleaseStamp(stamps[i]);
        stamps.Clear();
    }
}
