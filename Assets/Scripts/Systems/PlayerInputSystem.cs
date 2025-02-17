using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

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

        foreach (RefRW<PlayerInput> playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.Move = moveValue;
            playerInput.ValueRW.Zoom = -1 * zoomValue;
        }
    }

    protected override void OnDestroy()
    {
        _controls.Disable();
    }

    
}