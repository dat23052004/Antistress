using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentData", menuName = "ScriptableObjects/EnvironmentData")]
public class EnvironmentData : ScriptableObject
{
    public List<EnvironmentEntry> environments;

    public List<EnvironmentEntry> GetEntriesByType(EnvironmentType type)
    {
        List<EnvironmentEntry> results = new List<EnvironmentEntry>();

        if (environments == null)
            return results;

        foreach (var environment in environments)
        {
            if (environment != null && environment.type == type)
                results.Add(environment);
        }

        results.Sort((left, right) => left.environmentId.CompareTo(right.environmentId));
        return results;
    }

    public bool TryGetEntry(EnvironmentType type, int environmentId, out EnvironmentEntry entry)
    {
        if (environments != null)
        {
            foreach (var environment in environments)
            {
                if (environment != null &&
                    environment.type == type &&
                    environment.environmentId == environmentId)
                {
                    entry = environment;
                    return true;
                }
            }
        }

        entry = null;
        return false;
    }
}

[System.Serializable]
public class EnvironmentEntry
{
    [Header("Identity")]
    public int environmentId;
    public EnvironmentType type;
    public string displayName;
    public Sprite icon;

    [Header("Addressables")]
    public string environmentKey;

    [Header("Environment Settings")]
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
}

public enum EnvironmentType
{
    Menu,
    Game,
    Toy
}
