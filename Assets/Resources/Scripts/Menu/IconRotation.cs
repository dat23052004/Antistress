using DG.Tweening;
using UnityEngine;

public class IconRotation : MonoBehaviour
{
    [Header("Rotation")]
    public float minAngle = -45f;    // góc nghiêng tối thiểu (±)
    public float maxAngle = 45f;     // góc nghiêng tối đa (±)
    public float rotateDuration = 1f; // thời gian xoay 1 chiều

    [Header("Scale")]
    public float scaleMultiplier = 1.1f; // phóng to bao nhiêu %
    public float scaleDuration = 1f;     // thời gian phóng to/thu nhỏ

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;

        float randomDelay = Random.Range(0f, 0.5f); // lệch pha nhỏ
        Sequence seq = DOTween.Sequence();
        
        seq.Append(transform.DOLocalRotate(new Vector3(0, 0, maxAngle), rotateDuration).SetEase(Ease.InOutSine))
           .Append(transform.DOLocalRotate(new Vector3(0, 0, -maxAngle), rotateDuration * 2).SetEase(Ease.InOutSine))
           .Append(transform.DOLocalRotate(new Vector3(0, 0, maxAngle), rotateDuration * 2).SetEase(Ease.InOutSine))
           .SetLoops(-1, LoopType.Yoyo) // vô hạn
           .SetDelay(randomDelay);

        transform.DOScale(originalScale * scaleMultiplier, scaleDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetDelay(randomDelay + 0.2f);
    }

}
