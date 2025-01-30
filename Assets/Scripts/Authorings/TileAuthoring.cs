using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class TileAuthoring : MonoBehaviour
{
    public int TileType;
}

public class TileBaker : Baker<TileAuthoring>
{
    public override void Bake(TileAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Renderable);

        AddComponent(entity, new TileComponent
        {
            TileBlobRef = default, // Will be assigned later
            TileIndex = 0, // Will be assigned later
            WorldPosition = authoring.transform.position
        });
    }
}