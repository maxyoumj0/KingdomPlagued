using Unity.Entities;
using Unity.Transforms;

public struct BuildingPlacedComponent : IComponentData
{
    public int NetworkId;
    public LocalTransform LocalTransform;
}
