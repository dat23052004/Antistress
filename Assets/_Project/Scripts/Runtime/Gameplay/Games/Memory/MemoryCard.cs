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

    private MemoryGame _game;
    private CanvasGroup _canvasGroup;
    private bool _isAnimating;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(int cardType, Sprite icon, MemoryGame game)
    {
        CardType = cardType;
        _game = game;
        State = CardState.FaceDown;
        _isAnimating = false;

        if (_frontImage != null) _frontImage.sprite = icon;

        _backImage.enabled = true;
        _frontImage.enabled = false;

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
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

    public void SetMatched()
    {
        State = CardState.Matched;
        _canvasGroup.blocksRaycasts = false;
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 5, 0.5f);
        if (_frontImage != null)
            _frontImage.DOColor(new Color(0.85f, 1f, 0.85f), 0.2f);
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

        yield return transform.DOScaleX(0f, 0.1f).WaitForCompletion();

        State = toFaceUp ? CardState.FaceUp : CardState.FaceDown;
        _backImage.enabled = !toFaceUp;
        _frontImage.enabled = toFaceUp;

        yield return transform.DOScaleX(1f, 0.1f).SetEase(Ease.OutBack).WaitForCompletion();

        _isAnimating = false;
        _canvasGroup.blocksRaycasts = State == CardState.FaceDown;
    }
}
