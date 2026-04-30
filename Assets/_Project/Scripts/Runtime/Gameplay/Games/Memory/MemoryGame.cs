using System.Collections;
using TMPro;
using UnityEngine;

public class MemoryGame : MonoBehaviour, IEnvironmentStart
{
    private enum FlipState { WaitingFirstFlip, WaitingSecondFlip, Checking }

    [Header("References")]
    [SerializeField] private MemoryBoard _board;
    [SerializeField] private TextMeshProUGUI _moveCountText;

    private FlipState _state;
    private MemoryCard _firstCard;
    private int _moveCount;
    private int _matchedPairs;

    public IEnumerator OnEnvironmentReady()
    {
        _moveCount = 0;
        _matchedPairs = 0;
        _firstCard = null;
        UpdateMoveUI();

        yield return StartCoroutine(_board.SpawnBoard(this));
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
        yield return new WaitForSeconds(0.6f);

        if (_firstCard.CardType == second.CardType)
        {
            AudioManager.Ins.PlaySfx(SfxCue.Card_Match);
            _firstCard.SetMatched();
            second.SetMatched();
            _matchedPairs++;

            if (_matchedPairs >= _board.TotalPairs)
            {
                OnWin();
                yield break;
            }
        }
        else
        {
            AudioManager.Ins.PlaySfx(SfxCue.Card_Mismatch);
            _firstCard.FlipDown();
            second.FlipDown();
        }

        _firstCard = null;
        _state = FlipState.WaitingFirstFlip;
    }

    private void OnWin()
    {
        AudioManager.Ins.PlaySfx(SfxCue.Card_Win);
        // TODO: hook win UI khi có
    }

    public void Retry()
    {
        AudioManager.Ins.PlaySfx(SfxCue.UiClick);
        StartCoroutine(OnEnvironmentReady());
    }

    public void BackToMenu() => GameManager.Ins.BackToMenu();

    private void UpdateMoveUI()
    {
        if (_moveCountText != null)
            _moveCountText.text = $"Moves: {_moveCount}";
    }
}
