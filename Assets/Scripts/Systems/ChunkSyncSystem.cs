using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

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
            if (!chunk.ValueRO.IsLoaded)
                continue;

            // Send an RPC to inform the client to load this chunk
            Entity rpcEntity = ecb.CreateEntity();
            ecb.AddComponent(rpcEntity, new LoadChunkRpc
            {
                ChunkCoord = chunk.ValueRO.ChunkCoord
            });

            // Reset chunk status after sending
            ecb.SetComponentEnabled<ChunkComponent>(entity, false);
        }

        // TODO: Add logic for unloading chunks

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
