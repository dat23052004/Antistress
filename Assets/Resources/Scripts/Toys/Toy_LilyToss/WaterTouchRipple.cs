using UnityEngine;

public class WaterTouchRipple : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public ParticleSystem touchRipplePrefab;
    public LayerMask waterMask;

    public float minDistance = 0.7f;
    private Vector3 lastRipplePos = Vector3.one * 9999f;

    private void Reset()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (InputManager.Ins.TryGetPrimaryPointerDownThisFrame(out Vector2 screenPos))
            SpawnRippleAtScreenPos(screenPos);
    }

    private void SpawnRippleAtScreenPos(Vector3 screenPos)
    {
        if (cam == null)
            cam = Camera.main;

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        LayerMask lilyMask = LayerMask.GetMask("Lily");
        Collider2D hitLily = Physics2D.OverlapPoint(worldPos, lilyMask);
        if (hitLily != null)
            return;

        if (waterMask.value != 0)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos, waterMask);
            if (hit == null)
                return;
        }

        if (Vector3.Distance(worldPos, lastRipplePos) < minDistance)
            return;

        lastRipplePos = worldPos;
        ParticleSystem ripple = Instantiate(touchRipplePrefab, worldPos, Quaternion.identity);
        ripple.Play();
        Destroy(ripple.gameObject, 1f);
    }
}
