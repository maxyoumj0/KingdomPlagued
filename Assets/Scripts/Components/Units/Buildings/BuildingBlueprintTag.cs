using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class BuildingBlueprintTagAuthoring: MonoBehaviour
{
    public BuildingEnum BuildingEnum;

    class Baker : Baker<BuildingBlueprintTagAuthoring>
    {
        public override void Bake(BuildingBlueprintTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingBlueprintTagComponent {
                BuildingEnum = authoring.BuildingEnum,
            });
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct BuildingBlueprintTagComponent : IComponentData
{
    public BuildingEnum BuildingEnum;
}
