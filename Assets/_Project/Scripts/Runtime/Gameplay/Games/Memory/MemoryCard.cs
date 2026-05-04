using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MemoryCard : MonoBehaviour, IPointerClickHandler
{
    public enum CardState { FaceDown, FaceUp, Matched }

    [SerializeField] private Image _backImage;
    [SerializeField] private Image _frontImage;

    public int CardType { get; private set; }
    public CardState State { get; private set; }

    private MemoryGame   _game;
    private CanvasGroup  _canvasGroup;
    private RectTransform _rectTransform;
    private bool          _isAnimating;
    public  bool          IsFlipping => _isAnimating;

    private Tween    _spawnTween;
    private Sequence _flipSequence;
    private Sequence _matchSequence;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup   = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(int cardType, Sprite icon, MemoryGame game, float size)
    {
        CardType = cardType;
        _game    = game;
        State    = CardState.FaceDown;
        _isAnimating = false;

        if (_frontImage != null) _frontImage.sprite = icon;

        _backImage.enabled  = true;
        _frontImage.enabled = false;

        _rectTransform.sizeDelta = new Vector2(size, size);

        float randomZ = Random.Range(-10f, 10f);
        transform.localRotation = Quaternion.Euler(0f, 0f, randomZ);
        transform.localScale    = Vector3.zero;

        _spawnTween?.Kill();
        _spawnTween = transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
    }

    public void FlipUp()
    {
        if (State == CardState.FaceDown)
            StartCoroutine(DoFlip(true));
    }

    public void FlipDown()
    {
        if (State == CardState.FaceUp)
            StartCoroutine(DoFlip(false));
    }

    public void SetMatched(Vector3 targetWorldPos)
    {
        State = CardState.Matched;
        _canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();

        _matchSequence?.Kill();
        _matchSequence = DOTween.Sequence()
            // bay đến điểm tập kết
            .Append(transform.DOMove(targetWorldPos, 0.6f).SetEase(Ease.OutQuart))
            .Join(transform.DOScale(1.15f, 0.2f).SetEase(Ease.OutBack))
            .Append(transform.DOScale(1f, 0.1f))
            // float lên rồi fade out
            .Append(transform.DOMoveY(targetWorldPos.y + 180f, 0.85f).SetEase(Ease.InQuad))
            .Join(_canvasGroup.DOFade(0f, 0.85f).SetEase(Ease.InQuad))
            .OnComplete(() => gameObject.SetActive(false))
            .SetLink(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isAnimating || State != CardState.FaceDown) return;
        _game?.OnCardClicked(this);
    }

    private IEnumerator DoFlip(bool toFaceUp)
    {
        _isAnimating = true;
        _canvasGroup.blocksRaycasts = false;

        AudioManager.Ins.PlaySfx(SfxCue.Card_Flip);

        Vector2 originPos = _rectTransform.anchoredPosition;
        float   liftY     = 18f;
        float   driftX    = (Random.value < 0.5f ? -1f : 1f) * 10f;
        Vector2 liftedPos = originPos + new Vector2(driftX, liftY);

        _flipSequence?.Kill();
        _flipSequence = DOTween.Sequence()
            // nửa 1: thu scaleX về 0, đồng thời nhấc + nghiêng nhẹ
            .Append(transform.DOScaleX(0f, 0.18f).SetEase(Ease.InQuad))
            .Join(_rectTransform.DOAnchorPos(liftedPos, 0.18f).SetEase(Ease.OutQuad))
            // đổi mặt thẻ ở điểm giữa
            .AppendCallback(() =>
            {
                State               = toFaceUp ? CardState.FaceUp : CardState.FaceDown;
                _backImage.enabled  = !toFaceUp;
                _frontImage.enabled = toFaceUp;
            })
            // nửa 2: mở scaleX về 1, đồng thời hạ về vị trí gốc
            .Append(transform.DOScaleX(1f, 0.22f).SetEase(Ease.OutBack))
            .Join(_rectTransform.DOAnchorPos(originPos, 0.22f).SetEase(Ease.InQuad))
            .SetLink(gameObject);

        yield return _flipSequence.WaitForCompletion();

        _isAnimating = false;
        _canvasGroup.blocksRaycasts = State == CardState.FaceDown;
    }

    private void OnDestroy()
    {
        _spawnTween?.Kill();
        _flipSequence?.Kill();
        _matchSequence?.Kill();
    }
}
