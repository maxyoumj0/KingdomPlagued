using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class BuildingBlueprintInputAuthoring : MonoBehaviour
{
    class Baker : Baker<BuildingBlueprintInputAuthoring>
    {
        public override void Bake(BuildingBlueprintInputAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new BuildingBlueprintInputComponent
            {
                MousePos = float2.zero,
                LeftClick = 0.0f
            });
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct BuildingBlueprintInputComponent : IInputComponentData
{
    public float2 MousePos;
    public float LeftClick;
}
