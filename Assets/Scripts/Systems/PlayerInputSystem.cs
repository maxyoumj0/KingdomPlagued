using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
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
        float3 mousePos = float3.zero;
        float leftClick = _controls.Player.LeftClick.ReadValue<float>();

        EntityManager entityManager = World.EntityManager;

        foreach (DynamicBuffer<LinkedEntityGroup> linkedEntities in SystemAPI.Query<DynamicBuffer<LinkedEntityGroup>>().WithAll<GhostOwnerIsLocal>().WithAll<PlayerComponent>())
        {
            foreach (LinkedEntityGroup linkedEntity in linkedEntities)
            {
                if (entityManager.HasComponent<Camera>(linkedEntity.Value))
                {
                    Camera playerCamera = entityManager.GetComponentObject<Camera>(linkedEntity.Value);
                    mousePos = ScreenToWorld(_controls.Player.MousePosition.ReadValue<Vector2>(), playerCamera);
                }
            }
        }

        foreach (RefRW<PlayerInput> playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.Move = moveValue;
            playerInput.ValueRW.Zoom = -1 * zoomValue;
            playerInput.ValueRW.MousePos = mousePos;
            playerInput.ValueRW.LeftClick = leftClick;
        }
    }

    protected override void OnDestroy()
    {
        _controls.Disable();
    }

    private float3 ScreenToWorld(Vector2 screenPos, Camera camera)
    {
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        UnityEngine.Ray ray = camera.ScreenPointToRay(screenPos);
        RaycastInput rayInput = new RaycastInput
        {
            Start = ray.origin,
            End = ray.origin + (ray.direction * 100f), // Cast up to 100 units
            Filter = CollisionFilter.Default
        };
        if (physicsWorld.CastRay(rayInput, out Unity.Physics.RaycastHit hit))
        {
            Debug.Log($"Hit entity: {hit.Entity.Index}");
        } else
        {
            Debug.Log("Raycast missed!");
        }
        return float3.zero;
    }
}