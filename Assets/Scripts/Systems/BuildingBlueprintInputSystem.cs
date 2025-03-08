using Unity.Collections;
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
    private float _tileSize;

    protected override void OnCreate()
    {
        _controls = new Controls();
        _controls.Enable();
        _tileSize = 0;
        RequireForUpdate<PlayerComponent>();
        RequireForUpdate<MapSettingsComponent>();
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
        if (_tileSize == 0)
        {
            foreach(RefRO<MapSettingsComponent> mapSettings in SystemAPI.Query<RefRO<MapSettingsComponent>>())
            {
                _tileSize = mapSettings.ValueRO.TileSize;
            }
        }

        foreach ((RefRW<BuildingBlueprintInputComponent> playerInput, RefRO<LocalToWorld> transform, RefRO<BuildingBlueprintComponent> buildingBlueprint, Entity entity) in SystemAPI.Query<RefRW<BuildingBlueprintInputComponent>, RefRO<LocalToWorld>, RefRO<BuildingBlueprintComponent>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            playerInput.ValueRW.MousePos = ScreenToWorld(mousePos, _playerCamera, transform.ValueRO, SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld, buildingBlueprint.ValueRO);
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

    private float3 ScreenToWorld(float2 screenPos, Camera playerCamera, LocalToWorld transform, CollisionWorld collisionWorld, BuildingBlueprintComponent buildingBlueprint)
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
            int2 gridPos = WorldToGrid(hit.Position, _tileSize);
            int2 snappedGridPos = SnapToTile(gridPos, buildingBlueprint.numTilesX, buildingBlueprint.numTilesZ);
            return GridToWorld(snappedGridPos, _tileSize);
        }
        else
        {
            return transform.Position;
        }
    }

    private int2 SnapToTile(int2 gridPos, int width, int height)
    {
        int snappedX = gridPos.x - (width / 2);
        int snappedY = gridPos.y - (height / 2);
        return new int2(snappedX, snappedY);
    }

    private int2 WorldToGrid(float3 worldPos, float tileSize)
    {
        int gridX = Mathf.RoundToInt(worldPos.x / tileSize);
        int gridY = Mathf.RoundToInt(worldPos.z / tileSize);
        return new int2(gridX, gridY);
    }
    private float3 GridToWorld(int2 gridPos, float tileSize)
    {
        float worldX = gridPos.x * tileSize;
        float worldZ = gridPos.y * tileSize;
        return new float3(worldX, 0, worldZ); // Assuming ground level is Y=0
    }
}
