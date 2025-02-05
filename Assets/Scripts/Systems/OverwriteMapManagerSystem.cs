using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct OverwriteMapManagerSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Ensure system only runs if MapManagerComponent and PendingMapManagerSettingsComponent exist
        if (!SystemAPI.TryGetSingletonEntity<PendingMapManagerSettingsComponent>(out Entity pendingMapManagerSettingsEntity))
            return;
        if (!SystemAPI.TryGetSingletonEntity<MapManagerComponent>(out Entity mapManagerEntity))
            return;

        PendingMapManagerSettingsComponent pendingSettings = SystemAPI.GetComponent<PendingMapManagerSettingsComponent>(pendingMapManagerSettingsEntity);

        // Overwrite MapManager settings based on host's preference
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb.SetComponent(mapManagerEntity, new MapManagerComponent
        {
            Seed = pendingSettings.Seed,
            MapWidth = pendingSettings.MapWidth,
            MapHeight = pendingSettings.MapHeight,
            ChunkSize = pendingSettings.ChunkSize,
            TileSize = SystemAPI.GetComponentRO<MapManagerComponent>(mapManagerEntity).ValueRO.TileSize,
            TileDataBlob = SystemAPI.GetComponentRO<MapManagerComponent>(mapManagerEntity).ValueRO.TileDataBlob
        });
        Entity genMapentity = ecb.CreateEntity();
        ecb.AddComponent<GenServerMap>(genMapentity);
        ecb.Playback(state.EntityManager);
        state.Enabled = false;
    }
}
