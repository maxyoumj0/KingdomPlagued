using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TileAuthoring : MonoBehaviour
{
    public class Baker : Baker<TileAuthoring>
    {
        public override void Bake(TileAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new TileComponent {});
        }
    }
}

public struct TileComponent : IComponentData {}
