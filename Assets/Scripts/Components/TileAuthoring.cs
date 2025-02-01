using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class TileAuthoring : MonoBehaviour
{
    public class Baker : Baker<TileAuthoring>
    {
        public override void Bake(TileAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new TileComponent {});
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent]
public struct TileComponent : IComponentData {}
