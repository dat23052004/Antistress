using UnityEngine;

internal readonly struct GessoBrushPoint
{
    public readonly Vector2 worldPosition;
    public readonly float strokeDistance;
    public readonly float speed;
    public readonly float alpha01;

    public GessoBrushPoint(Vector2 worldPosition, float strokeDistance, float speed, float alpha01)
    {
        this.worldPosition = worldPosition;
        this.strokeDistance = strokeDistance;
        this.speed = speed;
        this.alpha01 = alpha01;
    }
}
