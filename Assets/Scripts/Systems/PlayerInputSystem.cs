using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class PlayerInputSystem : SystemBase
{
    private InputAction _moveAction;
    private InputAction _zoomAction;

    protected override void OnCreate()
    {
        _moveAction = InputSystem.actions.FindAction("Move", true);
        _moveAction?.Enable();

        _zoomAction = InputSystem.actions.FindAction("Zoom", true);
        _zoomAction?.Enable();
    }

    protected override void OnUpdate()
    {
        InputSystem.Update();
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        float zoomValue = _zoomAction.ReadValue<float>();
        Debug.Log($"ZoomeValue: {zoomValue}");

        foreach (RefRW<PlayerInput> playerInput in SystemAPI.Query<RefRW<PlayerInput>>().WithAll<GhostOwnerIsLocal>())
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