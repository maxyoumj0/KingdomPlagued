using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class PlayerInputSystem : SystemBase
{
    private InputAction _moveAction;
    private InputAction _zoomAction;

    protected override void OnCreate()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _moveAction?.Enable();

        _zoomAction = InputSystem.actions.FindAction("Zoom");
        _zoomAction?.Enable();
    }

    protected override void OnUpdate()
    {
        InputSystem.Update();
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        float zoomValue = _zoomAction.ReadValue<float>();

        foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW.Move = moveValue;
            playerInput.ValueRW.Zoom = zoomValue;
        }
    }

    protected override void OnDestroy()
    {
        _moveAction?.Disable();
        _zoomAction?.Disable();
    }
}