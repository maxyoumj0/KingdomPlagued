using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ChunkAuthoring : MonoBehaviour
{
    public bool IsLoaded = false;

    public class Baker : Baker<ChunkAuthoring>
    {
        public override void Bake(ChunkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ChunkComponent
            {
                ChunkCoord = default,
                IsLoaded = authoring.IsLoaded
            });
        }
    }
}

public struct ChunkComponent : IComponentData, IEnableableComponent
{
    public int2 ChunkCoord;
    public bool IsLoaded;
}
