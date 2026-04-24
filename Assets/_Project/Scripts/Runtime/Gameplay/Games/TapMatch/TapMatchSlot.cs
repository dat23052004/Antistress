using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TapMatchSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color occupiedColor = Color.white;

    public int TileType { get; private set; }
    public bool IsOccupied { get; private set; }
    
    private void Awake()
    {
        Clear();
    }

    public void Occupy(int tileType, Sprite icon)
    {
        TileType = tileType;
        IsOccupied = true;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = occupiedColor;
        }
    }

    public void Clear()
    {
        TileType = -1;
        IsOccupied = false;

        if (iconImage != null)
        {
            iconImage.sprite = emptySprite;
            iconImage.color = emptyColor;
        }
    }

    public void PunchScale()
    {
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 5, 0.5f);
    }

    public void PlayMatchAnimation(System.Action onComplete)
    {
        if (iconImage == null) { Clear(); onComplete?.Invoke(); return; }

        Sequence seq = DOTween.Sequence();
        seq.Append(iconImage.transform.DOScale(1.2f, 0.1f));
        seq.Append(iconImage.transform.DOScale(0f, 0.15f).SetEase(Ease.InBack));
        seq.OnComplete(() =>
        {
            iconImage.transform.localScale = Vector3.one;
            Clear();
            onComplete?.Invoke();
        });
    }

    public Sprite GetIcon() => iconImage != null ? iconImage.sprite : null;

    public RectTransform RectTransform => transform as RectTransform;
}
