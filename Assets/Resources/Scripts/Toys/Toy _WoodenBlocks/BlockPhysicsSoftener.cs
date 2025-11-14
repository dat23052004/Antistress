using UnityEngine;

public class BlockPhysicsSoftener : MonoBehaviour
{
    [Header("Velocity Limits")]
    public float maxLinearSpeed = 8f;
    public float maxAngularSpeed = 200f;

    [Header("Soft Collision Settings")]
    public float extraLinearDrag = 3f;
    public float extraAngularDrag = 4f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // preset kiểu Antistress
        rb.gravityScale = 1.1f;
        rb.mass = 0.45f;
        rb.linearDamping = extraLinearDrag;
        rb.angularDamping = extraAngularDrag;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // Clamp tốc độ để block không bị bắn
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxLinearSpeed);

        // Clamp tốc độ xoay
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularSpeed, maxAngularSpeed);
    }
}
