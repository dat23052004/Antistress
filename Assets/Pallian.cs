using Unity.VisualScripting;
using UnityEngine;

public class Pallian : MonoBehaviour
{
    private Rigidbody2D rb;
    private Camera cam;

    private bool isDragging;
    private Vector3 lastPointerWorld;
    private Vector3 pointerVelocity;

    private Vector2 lastThrowForce;
    private bool showDebugLine = true;
    private Vector2 dragTargetPos;

    public float followSpeed = 25f;
    public float throwMultiplier = 4f;
    public float throwThreshold = 3f;
    public float maxThrowForce = 100f;
    public float editorForceScale = 0.3f;

    public float linearWhileDrag = 5f;
    public float angularWhileDrag = 2f;
    public float linearDefault = 0f;
    public float angularDefault = 0f;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        rb.linearDamping = linearDefault;
        rb.angularDamping = angularDefault;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        HandlePointerInput();
    }
    private void FixedUpdate()
    {
        if (isDragging)
        {
            Vector2 posCorrection = dragTargetPos - rb.position;
            float posGain = Mathf.Clamp01(Time.fixedDeltaTime * followSpeed);
            rb.MovePosition(rb.position + posCorrection * posGain);
        }
    }

    public void HandlePointerInput()
    {
        bool pointerDown = false;
        bool pointerUp = false;
        bool pointerHeld = false;
        Vector3 pointerWorld = Vector3.zero;

#if UNITY_EDITOR || UNITY_STANDALONE
        pointerDown = Input.GetMouseButtonDown(0);
        pointerHeld = Input.GetMouseButton(0);
        pointerUp = Input.GetMouseButtonUp(0);
        pointerWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        pointerWorld.z = 0;

#else // MOBILE
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

        if (pointerDown)
        {
            Collider2D hit = Physics2D.OverlapPoint(pointerWorld);
            if (hit && hit.attachedRigidbody == rb)
            {
                isDragging = true;

                lastPointerWorld = pointerWorld;
                rb.linearDamping = linearWhileDrag;
                rb.angularDamping = angularWhileDrag;
            }
        }
        if (pointerHeld && isDragging)
        {
            Vector3 delta = pointerWorld - lastPointerWorld;
            pointerVelocity = delta / Mathf.Max(Time.deltaTime, 0.001f);
            lastPointerWorld = pointerWorld;

            // LƯU target position để FixedUpdate xử lý
            dragTargetPos = pointerWorld;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (pointerUp && isDragging)
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
