using System.Collections;
using UnityEngine;

public class PallinaEnvironment : MonoBehaviour, IEnvironmentStart
{
    public GameObject[] balls;
    public IEnumerator OnEnvironmentReady()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (var ball in balls)
        {
            Debug.Log(ball.name);
            Rigidbody2D rb = ball.GetComponentInChildren<Rigidbody2D>();
            if (rb == null) continue;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
