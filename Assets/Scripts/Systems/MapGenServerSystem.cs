using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct MapGenServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<MapManagerComponent>(out Entity mapManagerEntity))
        {
            return;
        }
        RefRO<MapManagerComponent> mapManagerComponent = SystemAPI.GetComponentRO<MapManagerComponent>(mapManagerEntity);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        // Handle GenServerMap from `MapGenServerSystem`
        foreach ((RefRO<GenServerMap> genServerMap, Entity entity) in SystemAPI.Query<RefRO<GenServerMap>>().WithEntityAccess())
        {
            GenerateWorld(ecb, mapManagerComponent);
            Entity serverMapGenDoneEntity = ecb.CreateEntity();
            ecb.AddComponent<ServerMapGenDone>(serverMapGenDoneEntity);
            ecb.DestroyEntity(entity);
        }

        // Handle seed request from `MapGenClientSystem`
        foreach ((RefRO<RequestMapManagerSettingsRpc> requestSeedRpc, Entity entity) in SystemAPI.Query<RefRO<RequestMapManagerSettingsRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Entity sendMapSettingsRpcEntity = ecb.CreateEntity();
            ecb.AddComponent(sendMapSettingsRpcEntity, new SendMapManagerSettingsRpc
            {
                ChunkSize = mapManagerComponent.ValueRO.ChunkSize,
                MapHeight = mapManagerComponent.ValueRO.MapHeight,
                MapWidth = mapManagerComponent.ValueRO.MapWidth,
                Seed = mapManagerComponent.ValueRO.Seed,
                TileSize = mapManagerComponent.ValueRO.TileSize
            });
            ecb.AddComponent<SendRpcCommandRequest>(sendMapSettingsRpcEntity);
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    private void GenerateWorld(EntityCommandBuffer ecb, RefRO<MapManagerComponent> mapManagerComponent)
    {
        int mapWidth = mapManagerComponent.ValueRO.MapWidth;
        int mapHeight = mapManagerComponent.ValueRO.MapHeight;
        float seed = mapManagerComponent.ValueRO.Seed;
        float tileSize = mapManagerComponent.ValueRO.TileSize;
        int totalTiles = mapWidth * mapHeight;

        using (BlobBuilder builder = new BlobBuilder(Allocator.Temp))
        {
            ref TileBlob tileBlob = ref builder.ConstructRoot<TileBlob>();
            BlobBuilderArray<TileData> tileArray = builder.Allocate(ref tileBlob.Tiles, totalTiles);

            int i = 0;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
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
