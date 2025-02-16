using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class BuildingBlueprintInputSystem : SystemBase
{
    private Controls _controls;

    protected override void OnCreate()
    {
        _controls = new Controls();
        _controls.Enable();
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<BuildingBlueprintInputComponent>();
        RequireForUpdate(GetEntityQuery(builder));
    }

    protected override void OnUpdate()
    {
        float2 mousePos = _controls.Player.MousePosition.ReadValue<Vector2>();
        float leftClick = _controls.Player.LeftClick.ReadValue<float>();

        foreach (RefRW<BuildingBlueprintInputComponent> playerInput in SystemAPI.Query<RefRW<BuildingBlueprintInputComponent>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.MousePos = mousePos;
            playerInput.ValueRW.LeftClick = leftClick;
        }
    }

    protected override void OnDestroy()
    {
        _controls.Disable();
    }
}
