using System.Collections;
using UnityEngine;

public class WoodenBlocksEnvironment : MonoBehaviour, IEnvironmentStart
{
    public GameObject[] blocks;
    public IEnumerator OnEnvironmentReady()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var block in blocks)
        {
            Rigidbody2D rb = block.GetComponentInChildren<Rigidbody2D>();
            if (rb == null) continue;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
