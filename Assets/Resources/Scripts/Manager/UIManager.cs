using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Main panels")]
    public GameObject menuPanel;
    public GameObject gameSelectionPanel;
    public GameObject toySelectionPanel;
    public GameObject gameplayPanel;
    public GameObject settingPanel;

    [Header("Game UI Panels")]
    public GameObject[] gameUIPanels;

    [Header("Toy UI panels")]
    public GameObject[] toyUIPanels;

    [Header("Common UI")]
    public GameObject backButton;
    public GameObject pauseButton;

    protected override void Initialize()
    {
        HideAllPanels();
        ShowMenu();
    }

    public void ShowMenu()
    {
        HideAllPanels();
        menuPanel.SetActive(true);
        backButton.SetActive(false);
    }

    public void ShowGameSelection()
    {
        HideAllPanels();
        gameSelectionPanel.SetActive(true);
        backButton.SetActive(true);
        pauseButton.SetActive(false);
    }

    public void ShowToySelection()
    {
        HideAllPanels();
        toySelectionPanel.SetActive(true);
        backButton.SetActive(true);
        pauseButton.SetActive(false);
    }

    public void ShowGameplay()
    {
        HideAllPanels();
        gameplayPanel.SetActive(true);
        backButton.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void ShowToyPlay() {         
        HideAllPanels();
        foreach (var panel in toyUIPanels)
        {
            panel.SetActive(true);
        }
        backButton.SetActive(true);
        pauseButton.SetActive(false);
    }   


    public void ShowGameUI(int gameIndex)
    {
        HideAllPanels();
        foreach (var panel in gameUIPanels)
        {
            panel.SetActive(true);
        }

       if(gameIndex < gameUIPanels.Length && gameUIPanels[gameIndex] != null)
        {
            gameUIPanels[gameIndex].SetActive(true);
        }
        backButton.SetActive(true);
        pauseButton.SetActive(false);
    }


    public void ShowToyUI(int toyIndex)
    {
        HideAllPanels();
        foreach (var panel in toyUIPanels)
        {
            panel.SetActive(true);
        }
        if (toyIndex < toyUIPanels.Length && toyUIPanels[toyIndex] != null)
        {
            toyUIPanels[toyIndex].SetActive(true);
        }
        backButton.SetActive(true);
        pauseButton.SetActive(false);
    }



    public void ShowSetting()
    {
        HideAllPanels();
        settingPanel.SetActive(true);
        backButton.SetActive(true);
        pauseButton.SetActive(false);
    }

    private void HideAllPanels()
    {
        menuPanel.SetActive(false);
        gameSelectionPanel.SetActive(false);
        toySelectionPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        settingPanel.SetActive(false);
        foreach (var panel in gameUIPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
        foreach (var panel in toyUIPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }
    }

    public void OneGameSelection(int gameIndex)
    {
        GameManager.Ins.StartGame(gameIndex);
        ShowGameUI(gameIndex);
    }

    public void OneToySelection(int toyIndex)
    {
        GameManager.Ins.StartToy(toyIndex);
        ShowToyUI(toyIndex);
    }


}

