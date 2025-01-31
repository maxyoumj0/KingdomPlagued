using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
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
        MapManagerComponent mapManager = SystemAPI.GetSingleton<MapManagerComponent>();
        int chunkSize = mapManager.ChunkSize;
        int mapWidth = mapManager.MapWidth;
        int mapHeight = mapManager.MapHeight;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        NativeHashSet<int2> activeChunks = new NativeHashSet<int2>(16, Allocator.Temp);

        // Get all players
        foreach ((RefRO<LocalTransform> localTransformComponent, Entity entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<PlayerComponent>().WithEntityAccess())
        {
            int2 playerChunkCoord = MapManagerHelper.WorldToChunkCoord(new float2(
                localTransformComponent.ValueRO.Position.x, localTransformComponent.ValueRO.Position.z), chunkSize, mapWidth, mapHeight);
            activeChunks.Add(playerChunkCoord);
        }

        // Load chunks if missing
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
                Entity chunkEntity = ecb.CreateEntity();
                ecb.AddComponent(chunkEntity, new ChunkComponent
                {
                    ChunkCoord = chunkCoord,
                    IsLoaded = true,
                });

                // TODO: Spawn tile entities for this chunk
            }
        }

        // Unload far chunks
        foreach (var (chunk, entity) in SystemAPI.Query<RefRO<ChunkComponent>>().WithEntityAccess())
        {
            if (!activeChunks.Contains(chunk.ValueRO.ChunkCoord))
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
