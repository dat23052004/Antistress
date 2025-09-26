using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    public List<GameEntry> games;
    public List<GameEntry> toys;
}

[System.Serializable]
public class GameEntry
{
    public string id;           // ID nội bộ
    public string displayName;  // Tên hiển thị
    public Sprite icon;         // Icon cho UI
    public GameObject uiPanel;  // UI panel hoặc prefab
}
