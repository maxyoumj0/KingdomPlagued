using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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

        foreach ((RefRO<LoadChunkRpc> rpc, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRpc>>().WithEntityAccess())
        {
            Debug.Log($"Loading chunk at {rpc.ValueRO.ChunkCoord}");

            // Check if chunk already exists
            bool chunkExists = false;
            foreach (var (chunk, chunkEntity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithEntityAccess())
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

        // TODO: Add logic for unloading chunks

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
