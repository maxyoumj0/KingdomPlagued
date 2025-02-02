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
        if (!SystemAPI.HasSingleton<MapManagerComponent>() || !SystemAPI.HasSingleton<PendingMapManagerSettingsComponent>())
            return;

        RefRW<MapManagerComponent> mapManager = SystemAPI.GetSingletonRW<MapManagerComponent>();

        PendingMapManagerSettingsComponent pendingSettings = SystemAPI.GetSingleton<PendingMapManagerSettingsComponent>();

        // Overwrite MapManager settings based on host's preference
        mapManager.ValueRW.ChunkSize = pendingSettings.ChunkSize;
        mapManager.ValueRW.MapWidth = pendingSettings.MapWidth;
        mapManager.ValueRW.MapHeight = pendingSettings.MapHeight;
        mapManager.ValueRW.Seed = pendingSettings.Seed;

        state.Enabled = false;
    }
}
