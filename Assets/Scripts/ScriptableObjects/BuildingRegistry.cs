using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRegistryData", menuName = "Scriptable Objects/BuildingRegistryData")]
public class BuildingRegistry : ScriptableObject
{
    [System.Serializable]
    public struct BuildingEntry
    {
        public BuildingEnum BuildingType;
        public GameObject Prefab;
    }

    public List<BuildingEntry> BuildingEntries;

    // Helper method to get the prefab by enum
    public GameObject GetPrefab(BuildingEnum buildingType)
    {
        foreach (var entry in BuildingEntries)
        {
            if (entry.BuildingType == buildingType)
            {
                return entry.Prefab;
            }
        }

        Debug.LogError($"Prefab for {buildingType} not found in BuildingRegistry!");
        return null;
    }
}
