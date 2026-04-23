using System.Collections;
using UnityEngine;

public class TapMatchGame : MonoBehaviour, IEnvironmentStart
{
    [SerializeField] private TapMatchBoard board;
    [SerializeField] private TapMatchSlotBar slotBar;

    public TapMatchSlotBar SlotBar => slotBar;

    public IEnumerator OnEnvironmentReady()
    {
        board.Init(this);
        slotBar.Init(this);

        yield return StartCoroutine(board.SpawnBoard());
    }

    public void OnMatchCleared() { }

    public void OnBoardCleared()
    {
        StartCoroutine(NextWave());
    }

    private IEnumerator NextWave()
    {
        yield return new WaitForSeconds(0.4f);
        yield return StartCoroutine(board.SpawnBoard());
    }

    public void OnBarFull() { }

    public void Retry()
    {
        AudioManager.Ins.PlaySfx(SfxCue.UiClick);
        StartCoroutine(OnEnvironmentReady());
    }

    public void BackToMenu()
    {
        GameManager.Ins.BackToMenu();
    }
}
