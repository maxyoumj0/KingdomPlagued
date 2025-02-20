using Unity.Entities;
using UnityEngine;

public static class BuildingPrefabHelper
{
    public static Entity BuildingEnumToEntity(BuildingPrefabComponent prefabRefs, BuildingEnum buildingEnum, bool isBlueprint)
    {
        if (buildingEnum == BuildingEnum.TestBuilding)
        {
            if (isBlueprint)
                return prefabRefs.TestBuildingBlueprintPrefab;
            else
                return prefabRefs.TestBuildingPrefab;
        }
        Debug.LogError("BuildingEnum has no respective entity");
        return new Entity();
    }
}
