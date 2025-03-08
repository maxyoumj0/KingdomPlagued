using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class BuildingAuthoring : MonoBehaviour
{
    public BuildingEnum BuildingEnum;
    public int MaxHealth;

    class Baker : Baker<BuildingAuthoring>
    {
        public override void Bake(BuildingAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingComponent
            {
                buildingEnum = authoring.BuildingEnum,
                maxHealth = authoring.MaxHealth,
                curHealth = authoring.MaxHealth
            });
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct BuildingComponent : IComponentData
{
    public BuildingEnum buildingEnum;
    public int maxHealth;
    public int curHealth;
}
