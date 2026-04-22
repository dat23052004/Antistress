using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [Header("Main panels")]
    public GameObject menuPanel;
    public GameObject gameplayPanel;

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
    }

    public void HideAllPanels()
    {
        if (menuPanel) menuPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(false);
    }

    public void OneGameSelection(int gameIndex)
    {
        Debug.Log("Selected game index: " + gameIndex);
        AudioManager.Ins.PlaySfx(SfxCue.UiClick);
        GameManager.Ins.StartGame(gameIndex);
    }

    public void OneToySelection(int toyId)
    {
        Debug.Log("Selected toy id: " + toyId);
        AudioManager.Ins.PlaySfx(SfxCue.UiClick);
        GameManager.Ins.StartToy(toyId);
    }

    public void RandomEnvironmentSelection()
    {
        Debug.Log("Selected random environment");
        AudioManager.Ins.PlaySfx(SfxCue.RandomSelect);
        GameManager.Ins.StartRandomEnvironment();
    }
}
