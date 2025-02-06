using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerComponent
            {
                OwnerNetworkId = 0
            });
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent]
public struct PlayerComponent : IComponentData
{
    [GhostField] public int OwnerNetworkId;
}