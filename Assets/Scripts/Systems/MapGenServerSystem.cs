using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

//[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct MapGenServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.TryGetSingleton(out MapDataComponent mapData))
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        // Handle GenServerMap from `MapGenServerSystem`
        foreach ((RefRO<MapSettingsComponent> mapManager, Entity managerEntity) in SystemAPI.Query<RefRO<MapSettingsComponent>>().WithEntityAccess())
        {
            Debug.Log("Generating server map");
            GenerateWorld(ecb, mapManager.ValueRO);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    // TODO: Turn this into a job
    private void GenerateWorld(EntityCommandBuffer ecb, MapSettingsComponent mapSettingsComponent)
    {
        int mapWidth = mapSettingsComponent.MapWidth;
        int mapHeight = mapSettingsComponent.MapHeight;
        float seed = mapSettingsComponent.Seed;
        float tileSize = mapSettingsComponent.TileSize;
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
            Entity mapDataEntity = ecb.CreateEntity();
            ecb.SetName(mapDataEntity, "MapData Entity");
            ecb.AddComponent(mapDataEntity, new MapDataComponent
            {
                TileDataBlob = blobReference,
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
