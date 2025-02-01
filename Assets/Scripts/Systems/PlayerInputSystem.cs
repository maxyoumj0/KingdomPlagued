using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct PlayerInputSystem : ISystem
{
    private InputAction _moveAction;
    private InputAction _zoomAction;

    public void OnCreate(ref SystemState state)
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _moveAction?.Enable();

        _zoomAction = InputSystem.actions.FindAction("Zoom");
        _zoomAction?.Enable();
    }

    public void OnUpdate(ref SystemState state)
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

    public void OnDestroy(ref SystemState state)
    {
        _moveAction?.Disable();
        _zoomAction?.Disable();
    }
}