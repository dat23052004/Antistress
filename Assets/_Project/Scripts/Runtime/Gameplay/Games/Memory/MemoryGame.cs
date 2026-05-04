using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MemoryGame : MonoBehaviour, IEnvironmentStart
{
    private enum FlipState { WaitingFirstFlip, WaitingSecondFlip, Checking }

    [Header("References")]
    [SerializeField] private MemoryBoard _board;
    [SerializeField] private TextMeshProUGUI _moveCountText;
    [SerializeField] private Button _resetButton;
    [SerializeField] private RectTransform[] _matchPoints; // 2 điểm tập kết ở giữa màn hình

    [Header("Levels")]
    [SerializeField] private List<MemoryLayout> _levels;
    [SerializeField] private int _startLevel = 0;

    private static readonly WaitForSeconds WaitMismatch  = new(0.7f);
    private static readonly WaitForSeconds WaitNextLevel = new(0.8f);

    private int _currentLevel;
    private FlipState _state;
    private MemoryCard _firstCard;
    private int _moveCount;
    private int _matchedPairs;

    public IEnumerator OnEnvironmentReady()
    {
        if (_resetButton != null) _resetButton.onClick.AddListener(OnResetClicked);
        _currentLevel = _startLevel;
        yield return StartCoroutine(LoadLevel(_currentLevel));
    }

    private void OnDestroy()
    {
        if (_resetButton != null) _resetButton.onClick.RemoveListener(OnResetClicked);
    }

    public void OnResetClicked()
    {
        if (_levels == null || _levels.Count == 0) return;
        AudioManager.Ins.PlaySfx(SfxCue.UiClick);
        _currentLevel = PickRandomOtherLevel();
        StartCoroutine(LoadLevel(_currentLevel));
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        _moveCount = 0;
        _matchedPairs = 0;
        _firstCard = null;
        UpdateMoveUI();

        MemoryLayout layout = GetLayout(levelIndex);
        yield return StartCoroutine(_board.SpawnBoard(this, layout));
        _state = FlipState.WaitingFirstFlip;
    }

    public void OnCardClicked(MemoryCard card)
    {
        if (_state == FlipState.Checking) return;
        if (card.State != MemoryCard.CardState.FaceDown) return;

        if (_state == FlipState.WaitingFirstFlip)
        {
            _firstCard = card;
            card.FlipUp();
            _state = FlipState.WaitingSecondFlip;
        }
        else
        {
            if (card == _firstCard) return;
            card.FlipUp();
            _moveCount++;
            UpdateMoveUI();
            StartCoroutine(CheckMatch(card));
        }
    }

    private IEnumerator CheckMatch(MemoryCard second)
    {
        _state = FlipState.Checking;
        yield return new WaitUntil(() => !second.IsFlipping);
        yield return WaitMismatch;

        if (_firstCard.CardType == second.CardType)
        {
            AudioManager.Ins.PlaySfx(SfxCue.Card_Match);
            AssignMatchPoints(_firstCard, second);
            _matchedPairs++;

            if (_matchedPairs >= _board.TotalPairs)
            {
                OnWin();
                yield break;
            }
        }
        else
        {
            _firstCard.FlipDown();
            second.FlipDown();
        }

        _firstCard = null;
        _state = FlipState.WaitingFirstFlip;
    }

    private void OnWin()
    {
        AudioManager.Ins.PlaySfx(SfxCue.Card_Win);
        StartCoroutine(NextLevel());
    }

    private IEnumerator NextLevel()
    {
        yield return WaitNextLevel;
        _currentLevel = PickRandomOtherLevel();
        yield return StartCoroutine(LoadLevel(_currentLevel));
    }

    public void Retry()
    {
        AudioManager.Ins.PlaySfx(SfxCue.UiClick);
        StartCoroutine(LoadLevel(_currentLevel));
    }

    public void BackToMenu() => GameManager.Ins.BackToMenu();

    private void AssignMatchPoints(MemoryCard cardA, MemoryCard cardB)
    {
        if (_matchPoints == null || _matchPoints.Length < 2)
        {
            cardA.SetMatched(cardA.transform.position);
            cardB.SetMatched(cardB.transform.position);
            return;
        }

        // card X nhỏ hơn → điểm X nhỏ hơn
        bool aIsLeft = cardA.transform.position.x <= cardB.transform.position.x;
        bool pt0IsLeft = _matchPoints[0].position.x <= _matchPoints[1].position.x;

        Vector3 ptForA = (aIsLeft == pt0IsLeft) ? _matchPoints[0].position : _matchPoints[1].position;
        Vector3 ptForB = (aIsLeft == pt0IsLeft) ? _matchPoints[1].position : _matchPoints[0].position;

        cardA.SetMatched(ptForA);
        cardB.SetMatched(ptForB);
    }

    private int PickRandomOtherLevel()
    {
        if (_levels.Count == 1) return 0;
        int next = Random.Range(0, _levels.Count - 1);
        if (next >= _currentLevel) next++;
        return next;
    }

    private MemoryLayout GetLayout(int index)
    {
        if (_levels == null || _levels.Count == 0)
        {
            Debug.LogError("[MemoryGame] Chưa assign layout nào vào _levels.");
            return null;
        }
        return _levels[Mathf.Clamp(index, 0, _levels.Count - 1)];
    }

    private void UpdateMoveUI()
    {
        if (_moveCountText != null)
            _moveCountText.text = $"Moves: {_moveCount}";
    }
}
