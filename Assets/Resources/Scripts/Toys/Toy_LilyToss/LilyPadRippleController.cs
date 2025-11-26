using UnityEngine;

public class LilyPadRippleController : MonoBehaviour
{
    public ParticleSystem touchRipple;
    public ParticleSystem dragRipple;
    public float minMoveDistance = 0.5f;
    public float minSpeedForRipple = 1.2f;
    public float rippleInterval = 0.22f;

    private Vector3 lastRipplePos;
    private float rippleTimer = 0f;

    private LilyPad lily;

    private void Awake()
    {
        lily = GetComponent<LilyPad>();
        lily.OnPointerDownEvent += DoTouchRipple;
        lily.OnPointerMoveEvent += DoDragRipple;
    }

    private void Update()
    {
        rippleTimer += Time.deltaTime;
    }

    private void DoTouchRipple(Vector3 pos)
    {
        SpawnRipple(touchRipple, pos);
        lastRipplePos = pos;
        rippleTimer = 0;
    }

    private void DoDragRipple(Vector3 pos, float speed)
    {

        if (Vector3.Distance(lastRipplePos, pos) < minMoveDistance)
            return;
        if (rippleTimer < rippleInterval)
            return;

        lastRipplePos = pos;
        rippleTimer = 0;
        SpawnRipple(dragRipple, pos);
    }

    private void SpawnRipple(ParticleSystem prefab, Vector3 pos)
    {
        Debug.Log("spawn ripple");
        var r = Instantiate(prefab, pos, Quaternion.identity);
        r.Play();
        Destroy(r.gameObject, 2f);
    }

}
