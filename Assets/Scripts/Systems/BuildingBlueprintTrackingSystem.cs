using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct BuildingBlueprintTrackingSystem : ISystem
{
    [BurstCompile]  
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        foreach (var (blueprintOwner, localTransform, inputComponent) in SystemAPI.Query<RefRO<GhostOwner>, RefRW<LocalTransform>, RefRO<BuildingBlueprintInputComponent>>().WithAll<Simulate>())
        {
            localTransform.ValueRW.Position = inputComponent.ValueRO.MousePos;
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
