using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        // On LoadChunkRpc
        foreach ((RefRO<LoadChunkRpc> rpc, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRpc>>().WithEntityAccess())
        {
            Debug.Log($"Loading chunk at {rpc.ValueRO.ChunkCoord}");

            // Check if chunk already exists
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

                // TODO: Spawn tile entities for this chunk
            }

            ecb.DestroyEntity(entity);
        }

        // On UnloadChunkRpc
        foreach ((RefRO<UnloadChunkRpc> rpc, Entity entity) in SystemAPI.Query<RefRO<UnloadChunkRpc>>().WithEntityAccess())
        {
            Debug.Log($"Unloading chunk at {rpc.ValueRO.ChunkCoord}");

            // Check if chunk was already unloaded
            bool chunkUnloaded = true;
            foreach ((RefRO<ChunkComponent> chunk, Entity chunkEntity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithEntityAccess())
            {
                if (chunk.ValueRO.ChunkCoord.Equals(rpc.ValueRO.ChunkCoord))
                {
                    chunkUnloaded = false;
                    break;
                }
            }

            // Unload chunk if it's still loaded
            if (!chunkUnloaded)
            {
                ecb.DestroyEntity(entity);
                // TODO: Despawn tile entities for this chunk
            }
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
