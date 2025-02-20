using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BuildingBlueprintTagAuthoring: MonoBehaviour
{
    class Baker : Baker<BuildingBlueprintTagAuthoring>
    {
        public override void Bake(BuildingBlueprintTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BuildingBlueprintTagComponent {});
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct BuildingBlueprintTagComponent : IComponentData
{
    
}
