using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject drawerPrefab;
    [SerializeField] private Transform drawerContainer;
    [SerializeField] private GameObject drawerCustomerPrefab;
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private EnvironmentData environmentData;

    private readonly List<GameObject> spawnedDrawers = new List<GameObject>();

    private void Start()
    {
        LoadFallbackData();
        PopulateDrawers();
    }

    public void PopulateDrawers()
    {
        if (drawerPrefab == null || drawerContainer == null)
        {
            Debug.LogError("GameSelectionUI is missing drawer references.");
            return;
        }

        LoadFallbackData();
        ClearSpawnedDrawers();

        if (environmentData == null)
        {
            Debug.LogError("GameSelectionUI could not find EnvironmentData.");
            return;
        }

        BuildToyDrawers(environmentData.GetEntriesByType(EnvironmentType.Toy));

        if (drawerCustomerPrefab != null)
            SpawnDrawer(drawerCustomerPrefab);

        if (gameConfig == null)
        {
            Debug.LogWarning("GameSelectionUI could not find GameConfig. Game buttons were skipped.");
            return;
        }

        BuildGameDrawers(gameConfig.games);
    }

    private void BuildToyDrawers(List<EnvironmentEntry> toys)
    {
        for (int i = 0; i < toys.Count; i += 2)
        {
            GameObject drawer = SpawnDrawer(drawerPrefab);
            Button[] buttons = drawer.GetComponentsInChildren<Button>(true);

            ConfigureToyButton(buttons, 0, toys[i]);

            if (i + 1 < toys.Count)
                ConfigureToyButton(buttons, 1, toys[i + 1]);
            else
                HideButton(buttons, 1);
        }
    }

    private void BuildGameDrawers(List<GameEntry> games)
    {
        if (games == null)
            return;

        for (int i = 0; i < games.Count; i += 2)
        {
            GameObject drawer = SpawnDrawer(drawerPrefab);
            Button[] buttons = drawer.GetComponentsInChildren<Button>(true);

            ConfigureGameButton(buttons, 0, games[i], i);

            if (i + 1 < games.Count)
                ConfigureGameButton(buttons, 1, games[i + 1], i + 1);
            else
                HideButton(buttons, 1);
        }
    }

    private void ConfigureToyButton(Button[] buttons, int buttonIndex, EnvironmentEntry toy)
    {
        if (buttons.Length <= buttonIndex || toy == null)
            return;

        Button button = buttons[buttonIndex];
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();

        Image image = button.GetComponentInChildren<Image>();
        if (image != null)
            image.sprite = toy.icon;

        int toyId = toy.environmentId;
        button.onClick.AddListener(() => UIManager.Ins.OneToySelection(toyId));
        button.name = $"Toy_{toy.displayName}";
    }

    private void ConfigureGameButton(Button[] buttons, int buttonIndex, GameEntry game, int gameIndex)
    {
        if (buttons.Length <= buttonIndex || game == null)
            return;

        Button button = buttons[buttonIndex];
        button.gameObject.SetActive(true);
        button.onClick.RemoveAllListeners();

        Image image = button.GetComponentInChildren<Image>();
        if (image != null)
            image.sprite = game.icon;

        button.onClick.AddListener(() => UIManager.Ins.OneGameSelection(gameIndex));
        button.name = $"Game_{game.displayName}";
    }

    private void HideButton(Button[] buttons, int buttonIndex)
    {
        if (buttons.Length > buttonIndex)
            buttons[buttonIndex].gameObject.SetActive(false);
    }

    private GameObject SpawnDrawer(GameObject prefab)
    {
        GameObject drawer = Instantiate(prefab, drawerContainer);
        spawnedDrawers.Add(drawer);
        return drawer;
    }

    private void ClearSpawnedDrawers()
    {
        foreach (GameObject drawer in spawnedDrawers)
        {
            if (drawer != null)
                Destroy(drawer);
        }

        spawnedDrawers.Clear();
    }

    private void LoadFallbackData()
    {
        if (environmentData == null)
            environmentData = Resources.Load<EnvironmentData>("Prefabs/EnvironmentData");

        if (gameConfig == null)
            gameConfig = Resources.Load<GameConfig>("Prefabs/GameConfig");
    }
}
