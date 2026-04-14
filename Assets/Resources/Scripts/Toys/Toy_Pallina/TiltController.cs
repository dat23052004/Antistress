using UnityEngine;

public class TiltController : MonoBehaviour
{
    public static Vector2 CurrentGravity { get; private set; }

    [Header("Gravity Settings")]
    public float gravityScale = 9.81f;
    public float tiltMultiplier = 2f;
    public bool smoothGravity = true;
    public float smoothSpeed = 6f;
    public float deadZone = 0.05f;

    private Vector2 previousGravity;
    private Vector2 smoothedGravity;

    private void OnEnable()
    {
        previousGravity = Physics2D.gravity;
        Physics2D.gravity = Vector2.zero;
        CurrentGravity = Vector2.zero;
        smoothedGravity = Vector2.zero;
    }

    private void OnDisable()
    {
        Physics2D.gravity = previousGravity;
        CurrentGravity = Vector2.zero;
        smoothedGravity = Vector2.zero;
    }

    private void Update()
    {
        Vector2 tilt = InputManager.Ins.GetTilt();
        if (tilt.sqrMagnitude < deadZone * deadZone)
            tilt = Vector2.zero;

        Vector2 gravity2D = tilt * gravityScale * tiltMultiplier;

        if (smoothGravity)
            smoothedGravity = Vector2.Lerp(smoothedGravity, gravity2D, Time.deltaTime * smoothSpeed);
        else
            smoothedGravity = gravity2D;

        CurrentGravity = smoothedGravity;
        Physics2D.gravity = Vector2.zero;
    }
}
