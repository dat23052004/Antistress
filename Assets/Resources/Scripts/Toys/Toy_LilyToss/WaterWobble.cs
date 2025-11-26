using UnityEngine;

public class WaterWobble : MonoBehaviour
{
    public float amplitude = 0.03f;   // biên độ dao động
    public float speed = 1f;          // tốc độ
    public Vector2 direction = new Vector2(1, 0); // hướng

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
        direction.Normalize();
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = startPos + (Vector3)(direction * offset);
    }
}
