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
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (blueprintOwner, localTransform) in SystemAPI.Query<RefRO<GhostOwner>, RefRW<LocalTransform>>().WithAll<BuildingBlueprintTagComponent>())
        {
            foreach (var (playerInput, playerGhostOwner, playerTransform) in SystemAPI.Query<RefRO<PlayerInput>, RefRO<GhostOwner>, RefRO<LocalTransform>>())
            {
                if (blueprintOwner.ValueRO.NetworkId != playerGhostOwner.ValueRO.NetworkId)
                    continue;

                localTransform.ValueRW.Position = ScreenToWorld(playerInput.ValueRO.MousePos, playerTransform.ValueRO);
                Debug.Log($"localTransform.Position set to {localTransform.ValueRW.Position}");
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

    private float3 ScreenToWorld(float2 screenPos, LocalTransform playerTransform)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        RaycastInput rayInput = new RaycastInput
        {
            Start = playerTransform.Position,
            End = playerTransform.Position + (math.mul(playerTransform.Rotation, new float3(0, 0, 1)) * 1000f), // Cast up to 100 units
            Filter = CollisionFilter.Default
        };
        if (physicsWorld.CastRay(rayInput, out Unity.Physics.RaycastHit hit))
        {
            Debug.Log($"Hit entity: {hit.Entity.Index}");
            return hit.Position;
        }
        else
        {
            Debug.Log("Raycast missed!");
        }
        return float3.zero;
    }
}
