using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentData", menuName = "ScriptableObjects/EnvironmentData")]
public class EnvironmentData : ScriptableObject
{
    public List<EnvironmentEntry> environments;
}

[System.Serializable]
public class EnvironmentEntry
{
    public int environmentId;
    [Header("Addressables Key")]
    public string environmentKey; // key addressables

    public EnvironmentType type;

    [Header("Scene Objects")]
    public GameObject environmentPrefab;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public Material skybox;
    public AudioClip backgroundMusic;
    public Color ambientColor = Color.white;
}

public enum EnvironmentType
{
    Menu,
    Game,
    Toy
}

