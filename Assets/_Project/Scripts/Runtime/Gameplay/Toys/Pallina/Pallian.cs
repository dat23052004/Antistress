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

    [Header("Pointer Fallback")]
    [SerializeField] private bool allowPointerFallbackInEditor = true;
    [SerializeField] private bool allowPointerControlOnMobile = false;

    [Header("Collision Audio")]
    [SerializeField, Min(0f)] private float collisionSoundMinVelocity = 0.6f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // Pallina is driven by table tilt, not by Unity's global downward gravity.
        rb.gravityScale = 0f;
        rb.linearDamping = linearDefault;
        rb.angularDamping = angularDefault;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!CanUsePointerControl())
            return;

        HandlePointerInput();
    }

    private void FixedUpdate()
    {
        if (isDragging)
        {
            Vector2 posCorrection = dragTargetPos - rb.position;
            float posGain = Mathf.Clamp01(Time.fixedDeltaTime * followSpeed);
            rb.MovePosition(rb.position + posCorrection * posGain);
            return;
        }

        ApplyTiltForce();
    }

    public void HandlePointerInput()
    {
        if (cam == null)
            cam = Camera.main;

        bool pointerDown = false;
        bool pointerUp = false;
        bool pointerHeld = false;
        Vector3 pointerWorld = Vector3.zero;

        if (InputManager.TryGetPrimaryPointerDownThisFrame(out Vector2 pointerScreenPos))
        {
            pointerDown = true;
            pointerWorld = cam.ScreenToWorldPoint(pointerScreenPos);
            pointerWorld.z = 0f;
        }

        if (InputManager.TryGetPrimaryPointerHeld(out pointerScreenPos))
        {
            pointerHeld = true;
            pointerWorld = cam.ScreenToWorldPoint(pointerScreenPos);
            pointerWorld.z = 0f;
        }

        if (InputManager.TryGetPrimaryPointerUpThisFrame(out pointerScreenPos))
        {
            pointerUp = true;
            pointerWorld = cam.ScreenToWorldPoint(pointerScreenPos);
            pointerWorld.z = 0f;
        }

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

    private void ApplyTiltForce()
    {
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        Vector2 tiltGravity = TiltController.CurrentGravity;
        if (tiltGravity == Vector2.zero)
            return;

        Vector2 tiltForce = tiltGravity * rb.mass;
        rb.AddForce(tiltForce, ForceMode2D.Force);
    }

    private bool CanUsePointerControl()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return allowPointerFallbackInEditor;
#else
        return allowPointerControlOnMobile;
#endif
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!ShouldPlayBallCollisionSound(collision))
            return;

        AudioManager.Ins.PlaySfx(SfxCue.PallinaCollision);
    }

    private bool ShouldPlayBallCollisionSound(Collision2D collision)
    {
        if (collision == null || collision.rigidbody == null)
            return false;

        Pallian otherBall = collision.rigidbody.GetComponent<Pallian>();
        if (otherBall == null || otherBall == this)
            return false;

        if (GetInstanceID() > otherBall.GetInstanceID())
            return false;

        float minVelocitySqr = collisionSoundMinVelocity * collisionSoundMinVelocity;
        return collision.relativeVelocity.sqrMagnitude >= minVelocitySqr;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugLine || lastThrowForce == Vector2.zero)
            return;

        Gizmos.color = Color.red;
        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)lastThrowForce.normalized * Mathf.Log10(lastThrowForce.magnitude + 1f) * 2f;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.05f);
    }
}
