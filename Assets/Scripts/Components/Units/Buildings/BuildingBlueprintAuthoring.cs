using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class BuildingBlueprintAuthoring: MonoBehaviour
{
    public BuildingEnum BuildingEnum;
    public int NumTilesX;
    public int NumTilesZ;

    class Baker : Baker<BuildingBlueprintAuthoring>
    {
        public override void Bake(BuildingBlueprintAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingBlueprintComponent
            {
                buildingEnum = authoring.BuildingEnum,
                numTilesX = authoring.NumTilesX,
                numTilesZ = authoring.NumTilesZ,
            });
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct BuildingBlueprintComponent : IComponentData
{
    public BuildingEnum buildingEnum;
    public int numTilesX;
    public int numTilesZ;
}
