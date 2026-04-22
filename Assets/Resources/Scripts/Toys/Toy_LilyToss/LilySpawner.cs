using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LilySlot
{
    public Transform point;
    [HideInInspector] public bool occupied = false;
}


public class LilySpawner : MonoBehaviour
{
    [Header("Spawn Slots")]
    public LilySlot aceroSlot;
    public List<LilySlot> lilySlot;

    [Header("Prefabs")]
    public GameObject lilyPrefab1;
    public GameObject lilyPrefab2;
    public GameObject lilyPrefab3;
    public GameObject aceroPrefab;


    private void OnEnable()
    {
        LilyPad.OnLilyDestroyed += SpawnAtSlot;
    }

    private void OnDisable()
    {
        LilyPad.OnLilyDestroyed -= SpawnAtSlot;
    }

    private void Start()
    {
        SpawnAcero(aceroSlot);
        foreach (var slot in lilySlot)
            SpawnLilyAt(slot);
    }

    private void SpawnAtSlot(LilySlot slot)
    {
        if (slot == aceroSlot)
        {
            // maple không respawn
            slot.occupied = false;
            return;
        }

        SpawnLilyAt(slot);
    }

    private void SpawnAcero(LilySlot slot)
    {
        GameObject acero = Instantiate(aceroPrefab, aceroSlot.point.position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)), transform);
        StartCoroutine(ScaleUp(acero.transform, 1f, 0.25f));
        AudioManager.Ins.PlaySfx(SfxCue.LilySpawn);
        slot.occupied = true;

        acero.GetComponentInChildren<LilyPad>().mySlot = slot;
    }

    private void SpawnLilyAt(LilySlot slot)
    {
        if (slot.occupied) return;

        GameObject prefab = PickRandomPrefab();

        Vector2 offset = Random.insideUnitCircle * 0.1f;   // lệch nhẹ 0.25 đơn vị
        Vector3 pos = slot.point.position + (Vector3)offset;
        GameObject obj = Instantiate(
            prefab,
            pos,
            Quaternion.Euler(0, 0, Random.Range(0f, 360f)),
            transform
        );

        float s = Random.Range(0.55f, 0.7f);
        StartCoroutine(DisableCollisionTemp(obj, 0.25f));
        StartCoroutine(ScaleUp(obj.transform, s, 0.25f));
        AudioManager.Ins.PlaySfx(SfxCue.LilySpawn);

        // liệt kê slot
        slot.occupied = true;

        // truyền slot vào LilyPad
        obj.GetComponentInChildren<LilyPad>().mySlot = slot;
    }

    private GameObject PickRandomPrefab()
    {
        // 94% còn lại chia đều 3 lá lily
        float t = Random.value;
        if (t < 0.30f) return lilyPrefab1;
        if (t < 0.60f) return lilyPrefab2;
        return lilyPrefab3;
    }
    private IEnumerator ScaleUp(Transform target, float finalScale, float duration = 0.25f)
    {
        if(target == null) yield break;
        float t = 0f;
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.one * finalScale;

        target.localScale = start;

        while (t < duration)
        {
            if (target == null) yield break;
            t += Time.deltaTime;
            float k = t / duration;
            k = Mathf.SmoothStep(0f, 1f, k);   // easing đẹp
            target.localScale = Vector3.Lerp(start, end, k);
            yield return null;
        }

        target.localScale = end;

    }
    private IEnumerator DisableCollisionTemp(GameObject obj, float delay)
    {
        Collider2D col = obj.GetComponentInChildren<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
            yield return new WaitForSeconds(delay);  // thời gian scale xong
            col.enabled = true;
        }
    }

}
