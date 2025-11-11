using UnityEngine;

/// <summary>
/// Professional Drag & Throw Controller for both Mouse and Touch.
/// Handles natural feel, platform scaling, and debug visualization.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera cam;

    // --- Input state ---
    private bool isDragging;
    private Vector3 lastPointerWorld;
    private Vector3 pointerVelocity;
    private Vector2 grabOffset;

    // --- Debug ---
    private Vector2 lastThrowForce;
    private bool showDebugLine = true;

    [Header("Follow Settings")]
    [Tooltip("How fast block follows pointer.")]
    public float followSpeed = 25f;

    [Header("Throw Settings")]
    [Tooltip("Force multiplier when throwing.")]
    public float throwMultiplier = 6f;
    [Tooltip("Minimum speed of hand to count as throw.")]
    public float throwThreshold = 3f;
    [Tooltip("Maximum force allowed when throwing.")]
    public float maxThrowForce = 200f;

    [Header("Platform Force Scaling")]
    [Tooltip("Scale down throw force when testing in Editor (mouse input).")]
    public float editorForceScale = 0.3f; // reduce mouse sensitivity

    [Header("Damping")]
    [Tooltip("Damping while dragging.")]
    public float linearWhileDrag = 5f;
    public float angularWhileDrag = 2f;
    [Tooltip("Damping when released.")]
    public float linearDefault = 0.2f;
    public float angularDefault = 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        rb.linearDamping = linearDefault;
        rb.angularDamping = angularDefault;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        HandlePointerInput();
    }

    private void HandlePointerInput()
    {
        bool pointerDown = false;
        bool pointerHeld = false;
        bool pointerUp = false;
        Vector3 pointerWorld = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        // --- Mouse input ---
        pointerDown = Input.GetMouseButtonDown(0);
        pointerHeld = Input.GetMouseButton(0);
        pointerUp = Input.GetMouseButtonUp(0);
        pointerWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        pointerWorld.z = 0;
#else
        // --- Touch input ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointerWorld = cam.ScreenToWorldPoint(touch.position);
            pointerWorld.z = 0;

            if (touch.phase == TouchPhase.Began)
                pointerDown = true;
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                pointerHeld = true;
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                pointerUp = true;
        }
#endif

        // --- Khi bắt đầu chạm hoặc nhấn ---
        if (pointerDown)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerWorld);
            if (hit && hit.attachedRigidbody == rb)
            {
                isDragging = true;

                // ❗ LƯU OFFSET THEO LOCAL SPACE (điểm chạm trong toạ độ của block)
                grabOffset = transform.InverseTransformPoint(pointerWorld);

                lastPointerWorld = pointerWorld;
                rb.linearDamping = linearWhileDrag;
                rb.angularDamping = angularWhileDrag;
            }
        }

        // --- Khi giữ ---
        if (isDragging && pointerHeld)
        {
            Vector3 delta = pointerWorld - lastPointerWorld;
            pointerVelocity = delta / Mathf.Max(Time.deltaTime, 0.001f);
            lastPointerWorld = pointerWorld;

            // 1) THEO VỊ TRÍ: đẩy tâm block sao cho điểm nắm (sau khi xoay hiện tại) trùng đúng tay
            Vector2 grabWorld = (Vector2)transform.TransformPoint(grabOffset); // điểm nắm hiện tại (world)
            Vector2 posCorrection = (Vector2)pointerWorld - grabWorld;          // cần bù để đặt đúng dưới tay
            float posGain = Mathf.Clamp01(Time.deltaTime * followSpeed);
            rb.MovePosition(rb.position + posCorrection * posGain);

            // 2) THEO XOAY: để vector (tâm→điểm nắm) // song song // (tâm→tay)
            Vector2 vecGrab = grabWorld - rb.position;              // hướng điểm nắm hiện tại
            Vector2 vecPointer = (Vector2)pointerWorld - rb.position;  // hướng tay hiện tại
            float currentAngle = Mathf.Atan2(vecGrab.y, vecGrab.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(vecPointer.y, vecPointer.x) * Mathf.Rad2Deg;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            // PD controller để xoay mượt, không dao động
            const float springStrength = 0.35f;  // lực kéo về góc mong muốn
            const float dampingFactor = 0.06f;  // giảm chấn theo vận tốc góc
            float torque = (angleDiff * springStrength) - (rb.angularVelocity * dampingFactor);
            torque = Mathf.Clamp(torque, -10f, 10f); // giới hạn an toàn
            rb.AddTorque(torque, ForceMode2D.Force);
        }



        // --- Khi thả ---
        if (isDragging && pointerUp)
        {
            isDragging = false;
            rb.linearDamping = linearDefault;
            rb.angularDamping = angularDefault;

            // Xác định có ném hay chỉ thả
            float handSpeed = pointerVelocity.magnitude;
            if (handSpeed > throwThreshold)
            {
                float scale = Application.isEditor ? editorForceScale : 1f;
                Vector2 throwForce = Vector2.ClampMagnitude(pointerVelocity * throwMultiplier * scale, maxThrowForce);
                rb.AddForce(throwForce, ForceMode2D.Impulse);
                lastThrowForce = throwForce;
            }
            else
            {
                lastThrowForce = Vector2.zero;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugLine || lastThrowForce == Vector2.zero)
            return;

        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)lastThrowForce.normalized * Mathf.Log10(lastThrowForce.magnitude + 1) * 2f;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
