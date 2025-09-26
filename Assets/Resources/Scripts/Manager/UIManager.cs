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
        if (menuPanel != null)
            menuPanel.SetActive(true);
        if (backButton != null)
            backButton.SetActive(false);
    }

    public void ShowGameSelection()
    {
        HideAllPanels();
        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(true);
        if (backButton != null)
            backButton.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void ShowToySelection()
    {
        HideAllPanels();
        if (toySelectionPanel != null)
            toySelectionPanel.SetActive(true);
        if (backButton != null)
            backButton.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    public void ShowGameplay()
    {
        HideAllPanels();
        if (gameplayPanel != null)
            gameplayPanel.SetActive(true);
        if (backButton != null)
            backButton.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(true);
    }

    public void ShowToyPlay()
    {
        HideAllPanels();
        if (toyUIPanels != null)
        {
            foreach (var panel in toyUIPanels)
            {
                if (panel != null)
                    panel.SetActive(true);
            }
        }
        if (backButton != null)
            backButton.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }   


    public void ShowGameUI(int gameIndex)
    {
        HideAllPanels();

        if (gameUIPanels != null && gameIndex >= 0 && gameIndex < gameUIPanels.Length && gameUIPanels[gameIndex] != null)
        {
            gameUIPanels[gameIndex].SetActive(true);
        }

        if (backButton != null)
            backButton.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }


    public void ShowToyUI(int toyIndex)
    {
        HideAllPanels();

        if (toyUIPanels != null && toyIndex >= 0 && toyIndex < toyUIPanels.Length && toyUIPanels[toyIndex] != null)
        {
            toyUIPanels[toyIndex].SetActive(true);
        }

        if (backButton != null)
            backButton.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }



    public void ShowSetting()
    {
        HideAllPanels();
        if (settingPanel != null)
            settingPanel.SetActive(true);
        if (backButton != null)
            backButton.SetActive(true);
        if (pauseButton != null)
            pauseButton.SetActive(false);
    }

    private void HideAllPanels()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
        if (gameSelectionPanel != null)
            gameSelectionPanel.SetActive(false);
        if (toySelectionPanel != null)
            toySelectionPanel.SetActive(false);
        if (gameplayPanel != null)
            gameplayPanel.SetActive(false);
        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (gameUIPanels != null)
        {
            foreach (var panel in gameUIPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }

        if (toyUIPanels != null)
        {
            foreach (var panel in toyUIPanels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }
    }

    public void OneGameSelection(int gameIndex)
    {
        Debug.Log("Selected game index: " + gameIndex);
        GameManager.Ins.StartGame(gameIndex);
        ShowGameUI(gameIndex);
    }

    public void OneToySelection(int toyIndex)
    {
        Debug.Log("Selected toy index: " + toyIndex);
        GameManager.Ins.StartToy(toyIndex);
        ShowToyUI(toyIndex);
    }


}

