using UnityEngine;

public class WaterTouchRipple : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public ParticleSystem touchRipplePrefab;
    public LayerMask waterMask;

    public float minDistance = 0.7f;
    private Vector3 lastRipplePos = Vector3.one * 9999f;
    void Reset()
    {
        cam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            SpawnRippleAtScreenPos(Input.mousePosition);
        }
#endif
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                SpawnRippleAtScreenPos(t.position);
            }
        }
    }

    void SpawnRippleAtScreenPos(Vector3 screenPos)
    {
        if (cam == null) cam = Camera.main;
        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;

        LayerMask lilyMask = LayerMask.GetMask("Lily");
        var hitLily = Physics2D.OverlapPoint(worldPos, lilyMask);
        if (hitLily != null) return;

        if (waterMask.value != 0)
        {
            var hit = Physics2D.OverlapPoint(worldPos, waterMask);
            if (hit == null) return;
        }
        if (Vector3.Distance(worldPos, lastRipplePos) < minDistance)
        {
            return;
        }
        lastRipplePos = worldPos;
        ParticleSystem ripple = Instantiate(touchRipplePrefab, worldPos, Quaternion.identity);
        ripple.Play();
        Destroy(ripple.gameObject, 1f);
    }
}
