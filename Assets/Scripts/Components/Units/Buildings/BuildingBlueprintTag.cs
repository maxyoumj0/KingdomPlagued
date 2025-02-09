using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BuildingBlueprintTagAuthoring: MonoBehaviour
{
    class Baker : Baker<BuildingBlueprintTagAuthoring>
    {
        public override void Bake(BuildingBlueprintTagAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new BuildingBlueprintTagComponent {});
        }
    }
}

public struct BuildingBlueprintTagComponent : IComponentData
{
    
}
