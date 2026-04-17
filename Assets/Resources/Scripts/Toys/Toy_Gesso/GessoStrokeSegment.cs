using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class GessoStrokeSegment : MonoBehaviour
{
    private const float MinSegmentLengthSqr = 0.000001f;
    private static readonly Dictionary<Texture2D, Sprite> SpriteCache = new Dictionary<Texture2D, Sprite>();

    private Vector2 start;
    private Vector2 end;

    public void Initialize(
        Vector2 start,
        Vector2 end,
        float width,
        Color color,
        Texture2D stampTexture,
        Material material,
        string sortingLayerName,
        int sortingOrder,
        float stampSpacing,
        float stampOffsetJitter,
        Vector2 stampScaleRange,
        Vector2 stampAlphaRange,
        float stampAngleJitter)
    {
        this.start = start;
        this.end = end;

        Sprite stampSprite = GetOrCreateSprite(stampTexture);
        if (stampSprite == null)
            return;

        float distance = Vector2.Distance(start, end);
        Vector2 direction = distance > 0.0001f ? (end - start) / distance : Vector2.right;
        Vector2 normal = new Vector2(-direction.y, direction.x);

        float spacing = Mathf.Max(0.01f, stampSpacing);
        int stampCount = Mathf.Max(1, Mathf.CeilToInt(distance / spacing) + 1);
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float spriteWorldWidth = Mathf.Max(stampSprite.bounds.size.x, 0.0001f);
        float scaleBase = width / spriteWorldWidth;

        Vector2 alphaRange = SortRange(stampAlphaRange);
        Vector2 scaleRange = SortRange(stampScaleRange);

        for (int i = 0; i < stampCount; i++)
        {
            float t = stampCount == 1 ? 0.5f : i / (float)(stampCount - 1);
            Vector2 stampPosition = Vector2.Lerp(start, end, t);
            stampPosition += normal * Random.Range(-stampOffsetJitter, stampOffsetJitter);
            stampPosition += direction * Random.Range(-stampOffsetJitter * 0.35f, stampOffsetJitter * 0.35f);

            CreateStamp(
                stampSprite,
                stampPosition,
                baseAngle,
                scaleBase,
                color,
                material,
                sortingLayerName,
                sortingOrder,
                scaleRange,
                alphaRange,
                stampAngleJitter);
        }
    }

    public float DistanceTo(Vector2 point)
    {
        Vector2 segment = end - start;
        float segmentLengthSqr = segment.sqrMagnitude;

        if (segmentLengthSqr <= MinSegmentLengthSqr)
            return Vector2.Distance(point, start);

        float t = Vector2.Dot(point - start, segment) / segmentLengthSqr;
        t = Mathf.Clamp01(t);

        Vector2 closestPoint = start + segment * t;
        return Vector2.Distance(point, closestPoint);
    }

    private void CreateStamp(
        Sprite sprite,
        Vector2 position,
        float baseAngle,
        float scaleBase,
        Color baseColor,
        Material material,
        string sortingLayerName,
        int sortingOrder,
        Vector2 scaleRange,
        Vector2 alphaRange,
        float angleJitter)
    {
        GameObject stampObject = new GameObject("Stamp");
        stampObject.transform.SetParent(transform, false);
        stampObject.transform.position = new Vector3(position.x, position.y, transform.position.z);
        stampObject.transform.rotation = Quaternion.Euler(
            0f,
            0f,
            baseAngle + Random.Range(-angleJitter, angleJitter));

        float scaleX = scaleBase * Random.Range(scaleRange.x, scaleRange.y);
        float scaleY = scaleBase * Random.Range(scaleRange.x * 0.85f, scaleRange.y * 1.05f);
        stampObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        SpriteRenderer renderer = stampObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = sortingOrder;

        if (material != null)
            renderer.sharedMaterial = material;

        Color stampColor = baseColor;
        stampColor.a *= Random.Range(alphaRange.x, alphaRange.y);
        renderer.color = stampColor;
    }

    private static Sprite GetOrCreateSprite(Texture2D texture)
    {
        if (texture == null)
            return null;

        if (SpriteCache.TryGetValue(texture, out Sprite cachedSprite) && cachedSprite != null)
            return cachedSprite;

        Sprite runtimeSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);

        runtimeSprite.name = $"{texture.name}_RuntimeSprite";
        runtimeSprite.hideFlags = HideFlags.HideAndDontSave;
        SpriteCache[texture] = runtimeSprite;
        return runtimeSprite;
    }

    private static Vector2 SortRange(Vector2 range)
    {
        return range.x <= range.y
            ? range
            : new Vector2(range.y, range.x);
    }
}
