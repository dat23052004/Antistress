using UnityEngine;

public class TiltController : MonoBehaviour
{
    [Header("Gravity Settings")]
    public float gravityScale = 9.81f;     
    public float tiltMultiplier = 2f;     // tăng độ nhạy nghiêng
    public bool smoothGravity = true;     // làm mượt chuyển động
    public float smoothSpeed = 6f;

    private Vector2 targetGravity;

    void Update()
    {
        Vector3 accel = Input.acceleration;   

        // Chuyển từ 3D space → 2D gravity
        Vector2 gravity2D = new Vector2(accel.x, accel.y) * gravityScale * tiltMultiplier;

        if (smoothGravity)
        {
            targetGravity = Vector2.Lerp(targetGravity, gravity2D, Time.deltaTime * smoothSpeed);
            Physics2D.gravity = targetGravity;
        }
        else
        {
            Physics2D.gravity = gravity2D;
        }
    }
}
