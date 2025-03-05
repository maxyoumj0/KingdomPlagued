using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct BuildingBlueprintTrackingSystem : ISystem
{
    [BurstCompile]  
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        SystemAPI.TryGetSingleton(out NetworkTime networkTime);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (blueprintOwner, localTransform, inputComponent) in SystemAPI.Query<RefRO<GhostOwner>, RefRW<LocalTransform>, RefRO<BuildingBlueprintInputComponent>>().WithAll<Simulate>())
        {
            if (networkTime.IsFirstTimeFullyPredictingTick)
            {
                Debug.Log($"[{state.WorldUnmanaged.Name}] Tick: {networkTime.ServerTick.TickValue}, Position: {localTransform.ValueRO.Position}");
                localTransform.ValueRW.Position = inputComponent.ValueRO.MousePos;
            }

            if (inputComponent.ValueRO.LeftClick == 1.0f)
            {
                Entity placeBuildingEntity = ecb.CreateEntity();
                ecb.AddComponent<BuildingPlacedRpc>(placeBuildingEntity);
                ecb.AddComponent<SendRpcCommandRequest>(placeBuildingEntity);
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
