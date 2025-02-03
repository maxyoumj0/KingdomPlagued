using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct ChunkReceiverSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get tile data
        if (!SystemAPI.TryGetSingletonEntity<MapManagerComponent>(out Entity mapManagerEntity))
        {
            Debug.Log("MapManagerComponent not found in client world yet");
            return;
        }
        MapManagerComponent mapManager = SystemAPI.GetComponent<MapManagerComponent>(mapManagerEntity);
        if (!mapManager.TileDataBlob.IsCreated)
        {
            Debug.Log("TileDataBlob not created yet");
            return;
        }
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        ref TileBlob tileBlob = ref mapManager.TileDataBlob.Value;
        int chunkSize = mapManager.ChunkSize;
        int mapWidth = mapManager.MapWidth;
        PrefabReferencesComponent prefabRefEntity = SystemAPI.GetSingleton<PrefabReferencesComponent>();

        // On LoadChunkRpc
        foreach ((RefRO<LoadChunkRpc> rpc, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Debug.Log($"Loading chunk at {rpc.ValueRO.ChunkCoord}. ChunkSize:{chunkSize}. MapWidth:{mapWidth}. Seed:{mapManager.Seed}");

            // Check if this Chunk Entity already exists on client world
            bool chunkExists = false;
            foreach ((RefRO<ChunkComponent> chunk, Entity chunkEntity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithEntityAccess())
            {
                if (chunk.ValueRO.ChunkCoord.Equals(rpc.ValueRO.ChunkCoord))
                {
                    chunkExists = true;
                    break;
                }
            }

            if (!chunkExists)
            {
                // Create a new chunk on the client side
                Entity chunkEntity = ecb.CreateEntity();
                ecb.AddComponent(chunkEntity, new ChunkComponent
                {
                    ChunkCoord = rpc.ValueRO.ChunkCoord,
                    IsLoaded = true,
                });

                // Create entity command buffer to instantiate tiles in the new chunk
                for (int x = 0; x < chunkSize; x++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        // Calculate the index on tileBlob
                        int tileDataIndex = (rpc.ValueRO.ChunkCoord.y * chunkSize + z) * mapWidth + (rpc.ValueRO.ChunkCoord.x * chunkSize + x);
                        // Ensure index is valid
                        if (tileDataIndex >= tileBlob.Tiles.Length)
                        {
                            Debug.LogWarning($"Skipping invalid tile index: {tileDataIndex}");
                            continue;
                        }

                        TileData tileData = tileBlob.Tiles[tileDataIndex];

                        // Instantiate the correct tile prefab
                        Entity tilePrefab = MapManagerHelper.TileTypeToPrefab(tileData.TileType, prefabRefEntity);
                        Entity tileEntity = ecb.Instantiate(tilePrefab);

                        // Set the tile's position
                        ecb.SetComponent(tileEntity, new LocalTransform
                        {
                            Position = tileData.WorldPosition,
                            Rotation = SystemAPI.GetComponentRO<LocalTransform>(tilePrefab).ValueRO.Rotation,
                            Scale = SystemAPI.GetComponentRO<LocalTransform>(tilePrefab).ValueRO.Scale
                        });

                        // Add a tag to identify tiles in this chunk
                        ecb.AddComponent(tileEntity, new ChunkTileTag { ChunkCoord = rpc.ValueRO.ChunkCoord });
                    }
                }
            }
            ecb.DestroyEntity(entity);
        }

        // On UnloadChunkRpc
        foreach ((RefRO<UnloadChunkRpc> rpc, Entity rpcEntity) in SystemAPI.Query<RefRO<UnloadChunkRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Debug.Log($"Unloading chunk at {rpc.ValueRO.ChunkCoord}");

            // Unload chunk if it's still loaded
            foreach ((RefRO<ChunkTileTag> chunkTag, Entity chunkTagEntity) in SystemAPI.Query<RefRO<ChunkTileTag>>().WithEntityAccess())
            {
                if (chunkTag.ValueRO.ChunkCoord.Equals(rpc.ValueRO.ChunkCoord))
                {
                    ecb.DestroyEntity(chunkTagEntity);
                }
            }
            ecb.DestroyEntity(rpcEntity);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
