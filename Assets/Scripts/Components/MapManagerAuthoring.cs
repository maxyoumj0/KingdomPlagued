using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using UnityEngine;

public class MapManagerAuthoring : MonoBehaviour
{
    public GameObject DefaultTilePrefab;
    public int ChunkSize;
    public int MapWidth;
    public int MapHeight;
    public float Seed;

    public class Baker : Baker<MapManagerAuthoring>
    {
        public override void Bake(MapManagerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new MapManagerComponent
            {
                TileDataBlob = default,
                ChunkSize = authoring.ChunkSize,
                TileSize = authoring.DefaultTilePrefab.GetComponentInChildren<Renderer>().bounds.size.x,
                MapWidth = authoring.MapWidth,
                MapHeight = authoring.MapHeight,
                Seed = authoring.Seed,
            });
            AddComponent(entity, new GhostInstance());
        }
    }
}

[GhostComponent]
public struct MapManagerComponent : IComponentData
{
    public BlobAssetReference<TileBlob> TileDataBlob;  // Static Map Data
    [GhostField] public int ChunkSize;
    [GhostField] public float TileSize;
    [GhostField] public int MapWidth;
    [GhostField] public int MapHeight;
    [GhostField] public float Seed;
}

public struct TileBlob
{
    public BlobArray<TileData> Tiles;
}

public struct TileData
{
    public float3 WorldPosition;
    public TileType TileType;
    public BiomeType BiomeType;
}
public enum TileType { Grass, Water, Sand, Dirt, Stone }
public enum BiomeType { Plains, Desert, Ocean, Forest }