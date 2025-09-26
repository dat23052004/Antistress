using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectionUI : MonoBehaviour
{
    public GameObject drawerPrefab; // Prefab của ngăn kéo
    public Transform drawerContainer; // Nơi chứa các ngăn kéo
    public GameObject drawerCustomerPrefab;
    public GameConfig gameConfig; // Tham chiếu đến GameConfig ScriptableObject

    void Start()
    {
        PopulateDrawers();
    }

    public void PopulateDrawers()
    {

        // Xóa các ngăn kéo cũ
        foreach (Transform child in drawerContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < gameConfig.toys.Count; i += 2)
        {
            var drawer = Instantiate(drawerPrefab, drawerContainer);
            var buttons = drawer.GetComponentsInChildren<Button>();

            if (i < gameConfig.toys.Count)
            {
                var toy = gameConfig.toys[i];
                if (buttons.Length > 0)
                {
                    buttons[0].GetComponentInChildren<Image>().sprite = toy.icon;
                    int index = i; // tránh closure bug
                    buttons[0].onClick.AddListener(() => UIManager.Ins.OneToySelection(index));
                }
            }

            if (i + 1 < gameConfig.toys.Count)
            {
                var toy = gameConfig.toys[i + 1];
                if (buttons.Length > 1)
                {
                    buttons[1].GetComponentInChildren<Image>().sprite = toy.icon;
                    int index = i + 1;
                    buttons[1].onClick.AddListener(() => UIManager.Ins.OneToySelection(index));
                }
            }
            else
            {
                if (buttons.Length > 1)
                {
                    buttons[1].gameObject.SetActive(false);
                }
            }
        }

        Instantiate(drawerCustomerPrefab, drawerContainer);

        for (int i = 0; i < gameConfig.games.Count; i += 2)
        {
            var drawer = Instantiate(drawerPrefab, drawerContainer);
            var buttons = drawer.GetComponentsInChildren<Button>();

            // Button 1 (bên trái)
            if (i < gameConfig.games.Count)
            {
                var game = gameConfig.games[i];
                if (buttons.Length > 0)
                {
                    buttons[0].GetComponentInChildren<Image>().sprite = game.icon;
                    int index = i; // tránh closure bug
                    buttons[0].onClick.AddListener(() => UIManager.Ins.OneGameSelection(index));
                }
            }

            // Button 2 (bên phải)
            if (i + 1 < gameConfig.games.Count)
            {
                var game = gameConfig.games[i + 1];
                if (buttons.Length > 1)
                {
                    buttons[1].GetComponentInChildren<Image>().sprite = game.icon;
                    int index = i + 1;
                    buttons[1].onClick.AddListener(() => UIManager.Ins.OneGameSelection(index));
                }
            }
            else
            {
                // Nếu số game lẻ, disable button thứ 2
                if (buttons.Length > 1)
                {
                    buttons[1].gameObject.SetActive(false);
                }
            }

        }

    }



}
