using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct BuildingBlueprintTrackingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (blueprintOwner, localTransform) in SystemAPI.Query<RefRO<GhostOwner>, RefRW<LocalTransform>>().WithAll<BuildingBlueprintTagComponent>())
        {
            foreach (var (playerInput, playerGhostOwner) in SystemAPI.Query<RefRO<PlayerInput>, RefRO<GhostOwner>>())
            {
                if (blueprintOwner.ValueRO.NetworkId != playerGhostOwner.ValueRO.NetworkId)
                    continue;
                localTransform.ValueRW.Position = playerInput.ValueRO.MousePos;
                if (playerInput.ValueRO.LeftClick == 1.0f)
                {
                    
                    Entity placeBuildingEntity = ecb.CreateEntity();
                    ecb.AddComponent<BuildingPlacedRpc>(placeBuildingEntity);
                    ecb.AddComponent<SendRpcCommandRequest>(placeBuildingEntity);
                }
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
