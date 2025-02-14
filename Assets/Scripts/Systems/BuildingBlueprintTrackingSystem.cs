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
            foreach (var (playerInput, playerGhostOwner, playerTransform, playerEntity) in SystemAPI.Query<RefRO<PlayerInput>, RefRO<GhostOwner>, RefRO<LocalTransform>>().WithEntityAccess())
            {
                if (blueprintOwner.ValueRO.NetworkId != playerGhostOwner.ValueRO.NetworkId)
                    continue;

                localTransform.ValueRW.Position = ScreenToWorld(playerInput.ValueRO.MousePos, playerTransform.ValueRO, localTransform.ValueRO, playerEntity, state.EntityManager, collisionWorld);
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

    private float3 ScreenToWorld(float2 screenPos, LocalTransform playerTransform, LocalTransform blueprintTransform, Entity playerEntity, EntityManager entityManager, CollisionWorld collisionWorld)
    {
        Camera playerCamera = PlayerCameraHelper.GetPlayerCamera(playerEntity, entityManager);
        if (playerCamera == null)
        {
            Debug.LogError("No Camera found on player entity!");
            return blueprintTransform.Position;
        }
        UnityEngine.Ray ray = playerCamera.ScreenPointToRay(new float3(screenPos.x, screenPos.y, 0));

        // Create a RaycastInput for Unity.Physics
        RaycastInput rayInput = new RaycastInput
        {
            Start = ray.origin,
            End = ray.origin + ray.direction * 1000f, // Extend the ray into the world
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };

        // Debug visualization
        Debug.DrawLine(rayInput.Start, rayInput.End, Color.green, 5f);

        // Perform the raycast
        if (collisionWorld.CastRay(rayInput, out var hit))
        {
            Debug.Log($"Hit entity: {hit.Entity.Index} at {hit.Position}");
            return hit.Position; // Return the hit position
        }
        else
        {
            Debug.Log("Raycast missed!");
            return blueprintTransform.Position; // Fallback if no hit
        }
    }
}
