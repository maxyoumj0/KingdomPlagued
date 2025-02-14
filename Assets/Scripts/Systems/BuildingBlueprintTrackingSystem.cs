using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct BuildingBlueprintTrackingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        foreach (var (blueprintOwner, localTransform) in SystemAPI.Query<RefRO<GhostOwner>, RefRW<LocalTransform>>().WithAll<BuildingBlueprintTagComponent>())
        {
            foreach (var (playerInput, playerGhostOwner, playerTransform) in SystemAPI.Query<RefRO<PlayerInput>, RefRO<GhostOwner>, RefRO<LocalTransform>>())
            {
                if (blueprintOwner.ValueRO.NetworkId != playerGhostOwner.ValueRO.NetworkId)
                    continue;

                localTransform.ValueRW.Position = ScreenToWorld(playerInput.ValueRO.MousePos, playerTransform.ValueRO, localTransform.ValueRO, collisionWorld);
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

    private float3 ScreenToWorld(float2 screenPos, LocalTransform playerTransform, LocalTransform blueprintTransform, CollisionWorld collisionWorld)
    {
        float3 rayStart = playerTransform.Position;
        float3 rayDirection = math.forward(playerTransform.Rotation);

        RaycastInput rayInput = new RaycastInput
        {
            Start = rayStart,
            End = rayStart + rayDirection * 1000f,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };

        Debug.DrawLine(rayStart, rayInput.End, Color.green, 5f);

        if (collisionWorld.CastRay(rayInput, out var hit))
        {
            Debug.Log($"Hit entity: {hit.Entity.Index} at {hit.Position}");
            return hit.Position;
        }
        else
        {
            Debug.Log("Raycast missed!");
            return blueprintTransform.Position;
        }
    }
}
