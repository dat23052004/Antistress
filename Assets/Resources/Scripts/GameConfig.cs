using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ScriptableObjects/GameConfig", order = 1)]
public class GameConfig : ScriptableObject
{
    public List<GameEntry> games;
}

[System.Serializable]
public class GameEntry
{
    public string id;
    public string displayName;
    public Sprite icon;
}
