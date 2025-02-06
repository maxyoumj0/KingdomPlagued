using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

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
        foreach ((RefRO<LocalTransform> localTransformComponent, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>().WithEntityAccess())
        {
            Debug.Log("TEST");
        }

        //EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        //// Handle GenServerMap from `MapGenServerSystem`
        //foreach (var (mapManager, entity) in SystemAPI.Query<RefRO<MapManagerComponent>>().WithEntityAccess())
        //{
        //    Debug.Log($"Server Found MapManagerComponent on Entity {entity.Index}");
        //}

        //foreach ((RefRO<GenServerMap> genServerMap, Entity entity) in SystemAPI.Query<RefRO<GenServerMap>>().WithEntityAccess())
        //{
        //    foreach ((RefRO<MapManagerComponent>  mapManager, Entity managerEntity) in SystemAPI.Query<RefRO<MapManagerComponent>>().WithEntityAccess())
        //    {
        //        Debug.Log("Generating server map");
        //        GenerateWorld(ecb, mapManager.ValueRO);
        //        ecb.DestroyEntity(entity);
        //    }
        //}

        //ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    private void GenerateWorld(EntityCommandBuffer ecb, MapManagerComponent mapManagerComponent)
    {
        int mapWidth = mapManagerComponent.MapWidth;
        int mapHeight = mapManagerComponent.MapHeight;
        float seed = mapManagerComponent.Seed;
        float tileSize = mapManagerComponent.TileSize;
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
