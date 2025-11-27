using UnityEngine;
using UnityEngine.UI;

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
    private Vector2 dragTargetPos;
    // --- Debug ---
    private Vector2 lastThrowForce;
    private bool showDebugLine = true;

    [Header("Follow Settings")]
    [Tooltip("How fast block follows pointer.")]
    public float followSpeed = 150f;

    [Header("Throw Settings")]
    [Tooltip("Force multiplier when throwing.")]
    public float throwMultiplier = 4f;
    [Tooltip("Minimum speed of hand to count as throw.")]
    public float throwThreshold = 3f;
    [Tooltip("Maximum force allowed when throwing.")]
    public float maxThrowForce = 100f;

    [Header("Platform Force Scaling")]
    [Tooltip("Scale down throw force when testing in Editor (mouse input).")]
    public float editorForceScale = 0.3f; // reduce mouse sensitivity

    [Header("Damping")]
    [Tooltip("Damping while dragging.")]
    public float linearWhileDrag = 2.5f;
    public float angularWhileDrag = 2f;
    [Tooltip("Damping when released.")]
    public float linearDefault = 0.2f;
    public float angularDefault = 0.05f;

    public float springStrength = 0.42f;
    public float dampingFactor = 0.055f;
    public float maxTorque = 100f;
    public float gravityStrength = 3f;
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
    private void FixedUpdate()
    {
        if (!isDragging) return;

        // --- 1) FOLLOW POSITION (spring-damper) ---
        Vector2 grabWorld = transform.TransformPoint(grabOffset);
        Vector2 posCorrection = dragTargetPos - grabWorld;

        float posGain = Mathf.Clamp01(Time.fixedDeltaTime * followSpeed);
        rb.MovePosition(rb.position + posCorrection * posGain);


        // --- ROTATE AROUND GRAB POINT (true behavior) ---
        Vector2 vecGrab = grabWorld - rb.position;        // vector từ tâm → điểm chạm
        Vector2 vecPointer = dragTargetPos - rb.position;    // vector từ tâm → tay

        // góc hiện tại của điểm chạm
        float currentAngle = Mathf.Atan2(vecGrab.y, vecGrab.x);
        // góc mới theo tay
        float targetAngle = Mathf.Atan2(vecPointer.y, vecPointer.x);

        // chênh lệch góc
        float angleDiff = Mathf.DeltaAngle(currentAngle * Mathf.Rad2Deg, targetAngle * Mathf.Rad2Deg);

        // PD xoay chuẩn
        float torquePD = angleDiff * springStrength - rb.angularVelocity * dampingFactor;

        Vector2 grabDir = vecGrab.normalized;
        float gSign = Vector3.Cross(grabDir, Vector2.down).z;
        float gravityTorque = gSign * gravityStrength;

        // --- STATE SPLIT ---
        float pointerSpeed = pointerVelocity.magnitude;
        float torque;

        if (pointerSpeed > 0.15f)   
        {
            torque = torquePD;      
        }
        else                        
        {
            torque = -gravityTorque; 
        }
        rb.AddTorque(Mathf.Clamp(torque, -maxTorque, maxTorque),
                      ForceMode2D.Force);
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

            dragTargetPos = pointerWorld;

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
