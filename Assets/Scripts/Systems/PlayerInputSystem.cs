using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class PlayerInputSystem : SystemBase
{
    private Controls _controls;

    protected override void OnCreate()
    {
        _controls = new Controls();
        _controls.Enable();
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<PlayerInput>();
        RequireForUpdate(GetEntityQuery(builder));
    }

    protected override void OnUpdate()
    {
        Vector2 moveValue = _controls.Player.Move.ReadValue<Vector2>();
        float zoomValue = _controls.Player.Zoom.ReadValue<float>();
        float2 mousePos = _controls.Player.MousePosition.ReadValue<Vector2>();
        float leftClick = _controls.Player.LeftClick.ReadValue<float>();

        EntityManager entityManager = World.EntityManager;

        foreach ((RefRW<PlayerInput> playerInput, RefRO<LocalTransform> playerTransform) in SystemAPI.Query<RefRW<PlayerInput>, RefRO<LocalTransform>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.Move = moveValue;
            playerInput.ValueRW.Zoom = -1 * zoomValue;
            playerInput.ValueRW.MousePos = ScreenToWorld(mousePos, playerTransform.ValueRO);
            playerInput.ValueRW.LeftClick = leftClick;
        }
    }

    protected override void OnDestroy()
    {
        _controls.Disable();
    }

    private float3 ScreenToWorld(float2 screenPos, LocalTransform playerTransform)
    {

        if (!SystemAPI.HasSingleton<PhysicsWorldSingleton>())
        {
            Debug.LogError("PhysicsWorldSingleton is MISSING in the client world!");
        }
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        float3 screenPos3d = new(screenPos.x, pla);
        RaycastInput rayInput = new RaycastInput
        {
            Start = screenPos,
            End = screenPos + (math.forward(playerTransform.Rotation) * 1000f), // Cast up to 100 units
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };

        Debug.DrawRay(rayInput.Start, rayInput.End - rayInput.Start, Color.red, 5f);

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