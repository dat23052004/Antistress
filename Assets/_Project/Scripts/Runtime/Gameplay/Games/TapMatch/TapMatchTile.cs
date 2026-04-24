using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class TapMatchTile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;

    public int TileType { get; private set; }
    public bool IsCollected { get; private set; }
    public bool IsBlocked { get; private set; }
    public Sprite Icon => iconImage != null ? iconImage.sprite : null;

    private TapMatchBoard board;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(int tileType, Sprite icon, TapMatchBoard ownerBoard)
    {
        TileType = tileType;
        board = ownerBoard;
        IsCollected = false;

        if (iconImage != null) iconImage.sprite = icon;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    public void SetBlocked(bool blocked)
    {
        IsBlocked = blocked;
        canvasGroup.blocksRaycasts = !blocked;
        if (iconImage != null)
            iconImage.color = blocked ? new Color(0.3f, 0.3f, 0.3f) : Color.white;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsCollected || IsBlocked) return;
        board?.OnTileClicked(this);
    }

    // Bay đến vị trí slot, sau đó tự destroy
    public IEnumerator AnimateToSlot(Vector3 worldTarget)
    {
        IsCollected = true;
        canvasGroup.blocksRaycasts = false;

        // Reparent vào root Canvas để render đè lên tất cả trong khi bay
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.rootCanvas.transform, true);
            transform.SetAsLastSibling();
        }

        yield return transform.DOMove(worldTarget, 0.25f)
            .SetEase(Ease.InOutQuad)
            .WaitForCompletion();

        Destroy(gameObject);
    }
}
