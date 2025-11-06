using NUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Main panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;
    public GameObject settingPanel;

    public void ShowMenu()
    {
        HideAllPanels();
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void ShowGameplay()
    {
        HideAllPanels();
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
    }

    public void ShowSetting()
    {
        HideAllPanels();
        settingPanel.SetActive(true);

    }

    public void HideAllPanels()
    {
        if (menuPanel) menuPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(false);
        if (settingPanel) settingPanel.SetActive(false);
    }
    public void OneGameSelection(int gameIndex)
    {
        Debug.Log("Selected game index: " + gameIndex);
        GameManager.Ins.StartGame(gameIndex);
    }

    public void OneToySelection(int toyIndex)
    {
        Debug.Log("Selected toy index: " + toyIndex);
        GameManager.Ins.StartToy(toyIndex);
    }

}

