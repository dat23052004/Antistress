using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Menu, GameSelection, ToySelection, InGame, InToy, Settings, Paused }

public class GameManager : Singleton<GameManager>
{
    [Header("Game States")]
    public GameState currentState;

    [Header("Environment References")]
    public Transform[] gameEnvironments;
    public Transform[] toyEnvironment;
    public Transform menuEnvironment;


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
                CameraManager.Ins.MoveToMenuPosition();
                break;
            case GameState.GameSelection:
                UIManager.Ins.ShowGameSelection();
                break;
            case GameState.ToySelection:
                UIManager.Ins.ShowToySelection();
                break;
            case GameState.InGame:
                UIManager.Ins.ShowGameplay();
                break;
            case GameState.InToy:
                UIManager.Ins.ShowGameplay();
                break;
            case GameState.Settings:
                // Show settings panel
                break;
            case GameState.Paused:
                // Pause the game
                break;
        }
    }
    public void StartGame(int gameIndex)
    {
        EnvironmentManager.Ins.SwitchToGame(gameIndex);
        SwitchState(GameState.InGame);
    }
    public void StartToy(int toyIndex)
    {
        EnvironmentManager.Ins.SwitchToToy(toyIndex);
        SwitchState(GameState.InGame);
    }
    public void BackToMenu()
    {
        EnvironmentManager.Ins.SwitchToMenu();
        SwitchState(GameState.Menu);
    }

}
