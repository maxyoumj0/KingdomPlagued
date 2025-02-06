using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

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
        if (!SystemAPI.TryGetSingletonEntity<PrefabReferencesComponent>(out Entity prefabRefEntity))
            return;
        if (!SystemAPI.TryGetSingletonEntity<MapManagerComponent>(out Entity mapManagerEntityDD))
            return;

        Debug.Log("Received PendingMapManagerSettingsComponent in OverwriteMapManagerSettingsComponent");
        PendingMapManagerSettingsComponent pendingSettings = SystemAPI.GetComponent<PendingMapManagerSettingsComponent>(pendingMapManagerSettingsEntity);

        // Overwrite MapManager settings based on host's preference
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        PrefabReferencesComponent prefabRefComponent = SystemAPI.GetComponent<PrefabReferencesComponent>(prefabRefEntity);
        Entity mapManagerEntity = ecb.Instantiate(prefabRefComponent.MapManagerPrefab);
        var TileSize = SystemAPI.GetComponentRO<MapManagerComponent>(prefabRefComponent.MapManagerPrefab).ValueRO.TileSize;

        ecb.SetComponent(mapManagerEntity, new MapManagerComponent
        {
            Seed = pendingSettings.Seed,
            MapWidth = pendingSettings.MapWidth,
            MapHeight = pendingSettings.MapHeight,
            ChunkSize = pendingSettings.ChunkSize,
            TileSize = SystemAPI.GetComponentRO<MapManagerComponent>(prefabRefComponent.MapManagerPrefab).ValueRO.TileSize,
            TileDataBlob = SystemAPI.GetComponentRO<MapManagerComponent>(prefabRefComponent.MapManagerPrefab).ValueRO.TileDataBlob
        });
        Debug.Log("Instantiated MapManager");
        ecb.Playback(state.EntityManager);
        state.Enabled = false;
    }
}
