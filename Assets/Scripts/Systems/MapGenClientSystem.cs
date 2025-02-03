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
        if (!SystemAPI.TryGetSingletonEntity<MapManagerComponent>(out Entity mapManagerEntity))
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        // Handle client's request to generate the map on client side
        foreach ((RefRO<GenClientMap> genClientMapTag, Entity entity) in SystemAPI.Query<RefRO<GenClientMap>>().WithEntityAccess())
        {
            // Request for seed
            Entity requestSeedEntity = ecb.CreateEntity();
            ecb.AddComponent<RequestMapManagerSettingsRpc>(requestSeedEntity);
            ecb.AddComponent<SendRpcCommandRequest>(requestSeedEntity);
            ecb.DestroyEntity(entity);
        }

        // Receive seed
        foreach ((RefRO<SendMapManagerSettingsRpc> requestSeedRpc, Entity entity) in SystemAPI.Query<RefRO<SendMapManagerSettingsRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            // Set MapManagerComponent
            ecb.SetComponent(mapManagerEntity, new MapManagerComponent
            {
                Seed = requestSeedRpc.ValueRO.Seed,
                MapWidth = requestSeedRpc.ValueRO.MapWidth,
                MapHeight = requestSeedRpc.ValueRO.MapHeight,
                ChunkSize = requestSeedRpc.ValueRO.ChunkSize,
                TileSize = requestSeedRpc.ValueRO.TileSize,
                TileDataBlob = SystemAPI.GetComponentRO<MapManagerComponent>(mapManagerEntity).ValueRO.TileDataBlob
            });
            GenerateWorld(ecb, requestSeedRpc.ValueRO.MapWidth, requestSeedRpc.ValueRO.MapHeight, requestSeedRpc.ValueRO.Seed, requestSeedRpc.ValueRO.TileSize, requestSeedRpc.ValueRO.ChunkSize);
            Entity clientMapGenDoneEntity = ecb.CreateEntity();
            ecb.AddComponent<ClientMapGenDone>(clientMapGenDoneEntity);
            ecb.DestroyEntity(entity);
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

            // Create MapManager Singleton Entity
            Entity mapEntity = SystemAPI.GetSingletonEntity<MapManagerComponent>();
            ecb.SetComponent(mapEntity, new MapManagerComponent
            {
                TileDataBlob = blobReference,
                TileSize = tileSize,
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                Seed = seed,
                ChunkSize = chunkSize
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
