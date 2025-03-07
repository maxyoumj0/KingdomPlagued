using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class BuildingBlueprintInputSystem : SystemBase
{
    private Controls _controls;
    private Camera _playerCamera;

    protected override void OnCreate()
    {
        _controls = new Controls();
        _controls.Enable();
    }

    protected override void OnUpdate()
    {
        float2 mousePos = _controls.Player.MousePosition.ReadValue<Vector2>();
        float leftClick = _controls.Player.LeftClick.ReadValue<float>();
        if (_playerCamera == null)
        {
            Debug.Log("Camera rebuilding");
            _playerCamera = BuildPlayerCameraLookup();
        }

        foreach ((RefRW<BuildingBlueprintInputComponent> playerInput, RefRO<LocalToWorld> transform, Entity entity) in SystemAPI.Query<RefRW<BuildingBlueprintInputComponent>, RefRO<LocalToWorld>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            playerInput.ValueRW.MousePos = ScreenToWorld(mousePos, _playerCamera, transform.ValueRO, SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld);
            playerInput.ValueRW.LeftClick = leftClick;
        }
    }

    protected override void OnDestroy()
    {
        _controls.Disable();
    }

    private Camera BuildPlayerCameraLookup()
    {
        foreach((DynamicBuffer<LinkedEntityGroup> linkedEntities, Entity playerEntity) in SystemAPI.Query<DynamicBuffer<LinkedEntityGroup>>().WithAll<PlayerComponent, GhostOwnerIsLocal>().WithEntityAccess())
        {
            foreach (var linkedEntity in linkedEntities)
            {
                if (EntityManager.HasComponent<Camera>(linkedEntity.Value))
                {
                    return EntityManager.GetComponentObject<Camera>(linkedEntity.Value);
                }
            }
        }

        Debug.LogError("[BuildPlayerCameraLookup] No player camera found!");
        return null;
    }

    private float3 ScreenToWorld(float2 screenPos, Camera playerCamera, LocalToWorld transform, CollisionWorld collisionWorld)
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
            return hit.Position;
        }
        else
        {
            return transform.Position;
        }
    }
}
