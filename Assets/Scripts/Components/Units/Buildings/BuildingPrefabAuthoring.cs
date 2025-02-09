using Unity.Entities;
using UnityEngine;


public class BuildingPrefabAuthoring : MonoBehaviour
{
    public GameObject TestBuildingBlueprintPrefab;
    public GameObject TestBuildingPrefab;

    public class PrefabBaker : Baker<BuildingPrefabAuthoring>
    {
        public override void Bake(BuildingPrefabAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BuildingPrefabComponent
            {
                TestBuildingBlueprintPrefab = GetEntity(authoring.TestBuildingBlueprintPrefab, TransformUsageFlags.Dynamic),
                TestBuildingPrefab = GetEntity(authoring.TestBuildingPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct BuildingPrefabComponent : IComponentData
{
    public Entity TestBuildingBlueprintPrefab;
    public Entity TestBuildingPrefab;
}