using System.Globalization;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        // Ensure MapManagerComponent is all set
        if (!SystemAPI.TryGetSingletonEntity<MapSettingsComponent>(out Entity mapSettingsEntity))
            return;
        if (!SystemAPI.TryGetSingleton<MapDataComponent>(out MapDataComponent mapData))
            return;

        MapSettingsComponent mapSettings = SystemAPI.GetComponent<MapSettingsComponent>(mapSettingsEntity);
        int chunkSize = mapSettings.ChunkSize;
        int mapWidth = mapSettings.MapWidth;
        PrefabReferencesComponent prefabRefEntity = SystemAPI.GetSingleton<PrefabReferencesComponent>();
        quaternion tileRotation = SystemAPI.GetComponentRO<LocalTransform>(MapManagerHelper.TileTypeToPrefab(TileType.Grass, prefabRefEntity)).ValueRO.Rotation;
        float tileScale = SystemAPI.GetComponentRO<LocalTransform>(MapManagerHelper.TileTypeToPrefab(TileType.Grass, prefabRefEntity)).ValueRO.Scale;
        var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecbMain = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);

        // TODO: Figure out a more elegant way to handle multiple jobs
        NativeArray<JobHandle> jobHandles = new (100, Allocator.Temp);
        int numJobs = 0;

        // On LoadChunkRpc
        foreach ((RefRO<LoadChunkRpc> rpc, Entity entity) in SystemAPI.Query<RefRO<LoadChunkRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Debug.Log($"Loading chunk at {rpc.ValueRO.ChunkCoord}.");

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
                Entity chunkEntity = ecbMain.CreateEntity();
                ecbMain.AddComponent(chunkEntity, new ChunkComponent
                {
                    ChunkCoord = rpc.ValueRO.ChunkCoord,
                    IsLoaded = true,
                });
                EntityCommandBuffer.ParallelWriter ecbJob = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
                // Create entity command buffer to instantiate tiles in the new chunk
                CreateTilesJob job = new CreateTilesJob
                {
                    chunkSize = chunkSize,
                    mapWidth = mapWidth,
                    chunkCoord = rpc.ValueRO.ChunkCoord,
                    tileBlob = mapData.TileDataBlob,
                    prefabRefEntity = prefabRefEntity,
                    ecb = ecbJob,
                    tileRotation = tileRotation,
                    tileScale = tileScale
                };
                jobHandles[numJobs] = job.ScheduleParallel(chunkSize * chunkSize, 64, state.Dependency);
                numJobs++;
            }
            ecbMain.DestroyEntity(entity);
        }

        // On UnloadChunkRpc
        foreach ((RefRO<UnloadChunkRpc> rpc, Entity rpcEntity) in SystemAPI.Query<RefRO<UnloadChunkRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Debug.Log($"Unloading chunk at {rpc.ValueRO.ChunkCoord}");

            // Unload chunk if it's still loaded
            EntityCommandBuffer.ParallelWriter ecbJob = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var job = new DestroyTilesJob
            {
                targetChunkCoord = rpc.ValueRO.ChunkCoord,
                ecb = ecbJob
            };
            jobHandles[numJobs] = job.ScheduleParallel(state.Dependency);
            ecbMain.DestroyEntity(rpcEntity);
        }
        state.Dependency = JobHandle.CombineDependencies(jobHandles);
        jobHandles.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public struct CreateTilesJob : IJobFor
    {
        public int chunkSize;
        public int mapWidth;
        public int2 chunkCoord;
        public BlobAssetReference<TileBlob> tileBlob;
        public PrefabReferencesComponent prefabRefEntity;
        internal EntityCommandBuffer.ParallelWriter ecb;
        public quaternion tileRotation;
        public float tileScale;

        public void Execute(int index)
        {
            if (!tileBlob.IsCreated)
            {
                Debug.LogError("Tile Blob is not created!");
                return;
            }

            // Convert 1D index to 2D tile position within the chunk
            int x = index % chunkSize;
            int y = index / chunkSize;
            int tileDataIndex = (chunkCoord.y * chunkSize + y) * mapWidth + (chunkCoord.x * chunkSize + x);

            if (tileDataIndex >= tileBlob.Value.Tiles.Length)
            {
                Debug.LogWarning($"Skipping invalid tile index: {tileDataIndex}");
                return;
            }

            TileData tileData = tileBlob.Value.Tiles[tileDataIndex];

            // Instantiate the correct tile prefab
            Entity tilePrefab = MapManagerHelper.TileTypeToPrefab(tileData.TileType, prefabRefEntity);
            Entity tileEntity = ecb.Instantiate(index, tilePrefab);

            // Set the tile's position
            ecb.SetComponent(index, tileEntity, new LocalTransform
            {
                Position = tileData.WorldPosition,
                Rotation = tileRotation,
                Scale = tileScale
            });

            // Add a tag to identify tiles in this chunk
            ecb.AddComponent(index, tileEntity, new ChunkTileTag { ChunkCoord = chunkCoord });
        }
    }

    public partial struct DestroyTilesJob : IJobEntity
    {
        public int2 targetChunkCoord;
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, in ChunkTileTag chunkTag)
        {
            if (chunkTag.ChunkCoord.Equals(targetChunkCoord))
            {
                ecb.DestroyEntity(sortKey, entity);
            }
        }
    }
}
