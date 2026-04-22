using UnityEngine;

public enum GameState { Menu, InGame, InToy, Settings, Paused }

public class GameManager : Singleton<GameManager>
{
    [Header("Game States")]
    public GameState currentState;

    protected override void Initialize()
    {
        SwitchState(GameState.Menu);
    }

    public void SwitchState(GameState newState)
    {
        currentState = newState;
        switch (newState)
        {
            case GameState.Menu:
                UIManager.Ins.ShowMenu();
                break;
            case GameState.InGame:
                UIManager.Ins.ShowGameplay();
                break;
            case GameState.InToy:
                UIManager.Ins.ShowGameplay();
                break;
            case GameState.Settings:
                UIManager.Ins.ShowSetting();
                break;
        }
    }

    public void StartGame(int gameIndex)
    {
        EnvironmentManager.Ins.SwitchToGame(gameIndex);
        SwitchState(GameState.InGame);
    }

    public void StartToy(int toyId)
    {
        EnvironmentManager.Ins.SwitchToToy(toyId);
        SwitchState(GameState.InToy);
    }

    public void StartRandomEnvironment()
    {
        if (!EnvironmentManager.Ins.SwitchToRandomPlayableEnvironment(out EnvironmentType selectedType))
            return;

        if (selectedType == EnvironmentType.Game)
            SwitchState(GameState.InGame);
        else
            SwitchState(GameState.InToy);
    }

    public void BackToMenu()
    {
        AudioManager.Ins.PlaySfx(SfxCue.UiBack);
        EnvironmentManager.Ins.SwitchToMenu();
        SwitchState(GameState.Menu);
    }
}
