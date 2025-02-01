using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[RequireMatchingQueriesForUpdate]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ChunkSyncSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<ChunkComponent> chunk, Entity entity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithEntityAccess())
        {
            // Destroy `ChunkComponents` that are no longer active
            if (!chunk.ValueRO.IsLoaded)
            {
                Entity unloadRpcEntity = ecb.CreateEntity();
                // Send an RPC to inform the client to destroy tiles in this chunk
                ecb.AddComponent(unloadRpcEntity, new UnloadChunkRpc
                {
                    ChunkCoord = chunk.ValueRO.ChunkCoord
                });
                ecb.DestroyEntity(entity);
                continue;
            }

            // Send an RPC to inform the client to create tiles in this chunk
            Entity loadRpcEntity = ecb.CreateEntity();
            ecb.AddComponent(loadRpcEntity, new LoadChunkRpc
            {
                ChunkCoord = chunk.ValueRO.ChunkCoord
            });

            // Reset chunk status after sending
            ecb.SetComponentEnabled<ChunkComponent>(entity, false);
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
