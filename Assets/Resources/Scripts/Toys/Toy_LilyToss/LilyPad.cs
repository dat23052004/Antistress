using System;
using UnityEngine;

public class LilyPad : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera cam;

    // Drag state
    private bool isDragging = false;
    private Vector3 lastPointerWorld;
    private Vector3 pointerVelocity;
    private Vector2 grabOffset;
    private Vector2 dragTargetPos;

    // Debug
    private Vector2 lastThrowForce;
    private bool showDebugLine = true;

    public LayerMask lilyMask;

    [Header("Follow Settings")]
    public float followSpeed = 25f;

    [Header("Throw Settings")]
    public float throwMultiplier = 2.2f;
    public float throwThreshold = 2.5f;
    public float maxThrowForce = 40f;
    public float editorForceScale = 0.25f;

    [Header("Damping")]
    public float linearWhileDrag = 2f;
    public float angularWhileDrag = 0.8f;
    public float linearDefault = 0.6f;
    public float angularDefault = 0.3f;

    [Header("Water Drift")]
    public float driftInterval = 1.2f;
    public float driftForce = 0.15f;
    public float driftTorque = 0.1f;

    private float driftTimer = 0f;

    public static Action<LilySlot> OnLilyDestroyed;
    [HideInInspector] public LilySlot mySlot;

    public event Action<Vector3> OnPointerDownEvent;
    public event Action<Vector3, float> OnPointerMoveEvent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        rb.linearDamping = linearDefault;
        rb.angularDamping = angularDefault;
    }

    private void Update()
    {
        HandlePointerInput();
        HandleWaterDrift();
        CheckAutoDestroy();
    }

    private void FixedUpdate()
    {
        if (isDragging)
        {
            // Follow movement
            Vector2 grabWorld = transform.TransformPoint(grabOffset);
            Vector2 posCorrection = dragTargetPos - grabWorld;

            float posGain = Mathf.Clamp01(Time.fixedDeltaTime * followSpeed);
            rb.MovePosition(rb.position + posCorrection * posGain);

            // Rotation spring
            Vector2 vecGrab = grabWorld - rb.position;
            Vector2 vecPointer = dragTargetPos - rb.position;

            float currentAngle = Mathf.Atan2(vecGrab.y, vecGrab.x) * Mathf.Rad2Deg;
            float targetAngle = Mathf.Atan2(vecPointer.y, vecPointer.x) * Mathf.Rad2Deg;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            float springStrength = 0.23f;
            float dampingFactor = 0.32f;

            float torque = (angleDiff * springStrength) - (rb.angularVelocity * dampingFactor);
            torque = Mathf.Clamp(torque, -2.2f, 2.2f);

            rb.AddTorque(torque, ForceMode2D.Force);

            // Reset uncontrolled forces
            rb.linearVelocity = Vector2.zero;
        }
    }


    private void HandlePointerInput()
    {
        bool pointerDown = false;
        bool pointerHeld = false;
        bool pointerUp = false;

        Vector3 pointerWorld = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        pointerDown = Input.GetMouseButtonDown(0);
        pointerHeld = Input.GetMouseButton(0);
        pointerUp = Input.GetMouseButtonUp(0);
        pointerWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        pointerWorld.z = 0;
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointerWorld = cam.ScreenToWorldPoint(touch.position);
            pointerWorld.z = 0;

            pointerDown = touch.phase == TouchPhase.Began;
            pointerHeld = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            pointerUp = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }
#endif

        // BEGIN DRAG
        if (pointerDown)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerWorld, lilyMask);
            if (hit && hit.attachedRigidbody == rb)
            {
                isDragging = true;
                OnPointerDownEvent?.Invoke(pointerWorld);
                grabOffset = transform.InverseTransformPoint(pointerWorld);
                lastPointerWorld = pointerWorld;

                rb.linearDamping = linearWhileDrag;
                rb.angularDamping = angularWhileDrag;
            }
        }

        if (isDragging && pointerHeld)
        {
            Vector3 delta = pointerWorld - lastPointerWorld;
            pointerVelocity = delta / Mathf.Max(Time.deltaTime, 0.001f);
            lastPointerWorld = pointerWorld;

            float speed = pointerVelocity.magnitude;

            OnPointerMoveEvent?.Invoke(pointerWorld, speed);   // ⭐ GỌI LẠI Ở ĐÂY
            dragTargetPos = pointerWorld;
        }

        if (isDragging && pointerUp)
        {
            isDragging = false;

            rb.linearDamping = linearDefault;
            rb.angularDamping = angularDefault;

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


    private void HandleWaterDrift()
    {
        if (isDragging) return;

        driftTimer += Time.deltaTime;
        if (driftTimer >= driftInterval)
        {
            driftTimer = 0f;

            rb.AddForce(UnityEngine.Random.insideUnitCircle * driftForce, ForceMode2D.Force);
            rb.AddTorque(UnityEngine.Random.Range(-driftTorque, driftTorque), ForceMode2D.Force);
        }
    }
   

    private void CheckAutoDestroy()
    {
        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        if (vp.x < -0.11f || vp.x > 1.11f || vp.y < -0.11f || vp.y > 1.11f)
        {
            Destroy(transform.parent.gameObject);
        }
    }
    private void OnDestroy()
    {
        if (mySlot != null)
        {
            mySlot.occupied = false;
            OnLilyDestroyed?.Invoke(mySlot);
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
