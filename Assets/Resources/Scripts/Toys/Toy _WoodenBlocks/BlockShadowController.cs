using UnityEngine;
using UnityEngine.UI;

public class BlockDropShadow : MonoBehaviour
{
    public Transform dropShadowPrimary;
    public Transform dropShadowSecondary;

    public float radiusSPX = 0.18f;
    public float radiusSPY = 0.22f;

    public float radiusSSX = 0.2f;
    public float radiusSSY = 0.4f;

    void LateUpdate()
    {
        float rot = transform.eulerAngles.z;

        // normalize -180..180
        if (rot > 180) rot -= 360;

        float rad = rot * Mathf.Deg2Rad;

        float xA = Mathf.Sin(rad) * radiusSPX;
        float yA = Mathf.Cos(rad) * radiusSPY;

        if (dropShadowPrimary != null)
        {
            dropShadowPrimary.localPosition = new Vector3(xA, yA, 0);
            dropShadowPrimary.localRotation = Quaternion.identity;
        }

        if (dropShadowSecondary != null)
        {
            float xB = -Mathf.Sin(rad) * radiusSSX;
            float yB = -Mathf.Cos(rad) * radiusSSY;

            dropShadowSecondary.localPosition = new Vector3(xB, yB, 0);
            dropShadowSecondary.localRotation = Quaternion.identity;

        }
    }

    
}
