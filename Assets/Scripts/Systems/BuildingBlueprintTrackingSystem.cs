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
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        CollisionWorld collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        foreach (var (blueprintOwner, localTransform, inputComponent) in SystemAPI.Query<RefRO<GhostOwner>, RefRW<LocalTransform>, RefRO<BuildingBlueprintInputComponent>>().WithAll<Simulate>())
        {
            foreach (var (playerGhostOwner, playerEntity) in SystemAPI.Query<RefRO<GhostOwner>>().WithAll<PlayerComponent>().WithEntityAccess())
            {
                if (blueprintOwner.ValueRO.NetworkId != playerGhostOwner.ValueRO.NetworkId)
                    continue;
                Camera playerCamera = PlayerCameraHelper.GetPlayerCamera(playerEntity, state.EntityManager);
                if (playerCamera == null)
                {
                    Debug.LogError("No Camera found on player entity!");
                    continue;
                }
                var location = ScreenToWorld(inputComponent.ValueRO.MousePos, localTransform.ValueRO, playerCamera, collisionWorld);
                if (!location.Equals(Vector3.zero))
                {
                    localTransform.ValueRW.Position = location;
                    Debug.Log($"localTransform.Position set to {localTransform.ValueRW.Position}");
                }
                if (inputComponent.ValueRO.LeftClick == 1.0f)
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

    private float3 ScreenToWorld(float2 screenPos, LocalTransform blueprintTransform, Camera playerCamera, CollisionWorld collisionWorld)
    {
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

        // Perform the raycast
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
