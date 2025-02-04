using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ChunkLoaderSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get map settings
        if (!SystemAPI.TryGetSingletonEntity<MapManagerComponent>(out Entity mapManagerEntity))
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        // Wait until map finished generating on server world
        if (!SystemAPI.TryGetSingletonEntity<ServerMapGenDone>(out Entity serverMapGenDoneEntity))
            return;

        MapManagerComponent mapManager = SystemAPI.GetComponent<MapManagerComponent>(mapManagerEntity);
        int chunkSize = mapManager.ChunkSize;
        int mapWidth = mapManager.MapWidth;
        int mapHeight = mapManager.MapHeight;

        NativeHashSet<int2> activeChunks = new(16, Allocator.Temp);

        // Get all players
        foreach ((RefRO<LocalTransform> localTransformComponent, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>().WithEntityAccess())
        {
            // Skip on server's player entity
            if (SystemAPI.GetComponent<GhostOwner>(entity).NetworkId == 0)
            {
                continue;
            }

            // Calculate which chunk the player is on top of
            int2 playerChunkCoord = MapManagerHelper.WorldToChunkCoord(new float2(
                localTransformComponent.ValueRO.Position.x, localTransformComponent.ValueRO.Position.z), chunkSize, mapWidth, mapHeight);
            activeChunks.Add(playerChunkCoord);

            // TODO: Add logic for loading neighoring chunks as well based on player location
        }

        // Create ChunkComponent entity on server if needed
        foreach (int2 chunkCoord in activeChunks)
        {
            bool chunkExists = false;

            // Skip chunks that are already active
            foreach ((RefRO<ChunkComponent> chunk, Entity entity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithEntityAccess())
            {
                if (chunk.ValueRO.ChunkCoord.Equals(chunkCoord))
                {
                    chunkExists = true;
                    break;
                }
            }

            if (!chunkExists)
            {
                // Create Chunk Entity on the server 
                Entity chunkEntity = ecb.CreateEntity();
                ecb.AddComponent(chunkEntity, new ChunkComponent
                {
                    ChunkCoord = chunkCoord,
                    IsLoaded = false,
                });

                // Send `LoadChunkRpc` to client for them to load
                Entity loadRpcEntity = ecb.CreateEntity();
                ecb.AddComponent(loadRpcEntity, new LoadChunkRpc
                {
                    ChunkCoord = chunkCoord
                });
                ecb.AddComponent<SendRpcCommandRequest>(loadRpcEntity);
                Debug.Log($"LoadChunkRpc sent to client world");
            }
        }

        // Destroy chunks that should no longer be active
        foreach ((RefRW<ChunkComponent> chunk, Entity entity) in SystemAPI.Query<RefRW<ChunkComponent>>().WithEntityAccess())
        {
            if (!activeChunks.Contains(chunk.ValueRO.ChunkCoord))
            {
                // Send `UnloadChunkRpc` to the client to destroy chunks that are no longer needed
                Entity unloadRpcEntity = ecb.CreateEntity();
                ecb.AddComponent(unloadRpcEntity, new UnloadChunkRpc
                {
                    ChunkCoord = chunk.ValueRO.ChunkCoord
                });
                ecb.AddComponent<SendRpcCommandRequest>(unloadRpcEntity);

                // Destroy Chunks on server world
                ecb.DestroyEntity(entity);
            }
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
