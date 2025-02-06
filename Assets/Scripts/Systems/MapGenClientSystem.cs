using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct MapGenClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (RefRO<MapManagerComponent> mapManager in SystemAPI.Query<RefRO<MapManagerComponent>>())
        {
            // Ensure that TileDataBlob is generated on the server
            if (!mapManager.ValueRO.TileDataBlob.IsCreated)
                break;
            Debug.Log("GenerateWorld Client System");
            GenerateWorld(ecb, mapManager.ValueRO.MapWidth, mapManager.ValueRO.MapHeight, mapManager.ValueRO.Seed, mapManager.ValueRO.TileSize, mapManager.ValueRO.ChunkSize);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    private void GenerateWorld(EntityCommandBuffer ecb, int mapWidth, int mapHeight, float seed, float tileSize, int chunkSize)
    {
        int totalTiles = mapWidth * mapHeight;

        using (BlobBuilder builder = new BlobBuilder(Allocator.Temp))
        {
            ref TileBlob tileBlob = ref builder.ConstructRoot<TileBlob>();
            BlobBuilderArray<TileData> tileArray = builder.Allocate(ref tileBlob.Tiles, totalTiles);

            int i = 0;
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float noiseValue = noise.cnoise(new float2((x + seed) * 0.1f, (y + seed) * 0.1f));
                    TileType tileType = DetermineTileType(noiseValue);
                    BiomeType biomeType = DetermineBiome(noiseValue);

                    float curY = y * tileSize * 0.5f;
                    float curX = x * tileSize;
                    if (y % 2 != 0)
                    {
                        curX += tileSize * 0.5f;
                    }

                    float3 worldPosition = new float3(curX, 0, curY);

                    tileArray[i] = new TileData
                    {
                        WorldPosition = worldPosition,
                        TileType = tileType,
                        BiomeType = biomeType
                    };
                    i++;
                }
            }

            BlobAssetReference<TileBlob> blobReference = builder.CreateBlobAssetReference<TileBlob>(Allocator.Persistent);

            // Create MapManager Singleton Entity
            Entity mapEntity = SystemAPI.GetSingletonEntity<MapManagerComponent>();
            ecb.SetComponent(mapEntity, new MapManagerComponent
            {
                TileDataBlob = blobReference,
                ChunkSize = chunkSize,
                TileSize = tileSize,
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                Seed = seed
            });
        }
    }

    private TileType DetermineTileType(float noiseValue)
    {
        if (noiseValue < 0.3f) return TileType.Water;
        if (noiseValue < 0.6f) return TileType.Sand;
        return TileType.Grass;
    }

    private BiomeType DetermineBiome(float noiseValue)
    {
        if (noiseValue < 0.3f) return BiomeType.Ocean;
        if (noiseValue < 0.6f) return BiomeType.Desert;
        return BiomeType.Plains;
    }
}
