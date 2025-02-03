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
        Debug.Log($"moveValue:{moveValue}, zoomValue:{zoomValue}");

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

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct AddPlayerInputSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        bool hasUpdated = false;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRO<PlayerComponent> playerComponent, Entity entity) in SystemAPI.Query<RefRO<PlayerComponent>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<PlayerInput>(entity)) {
                
                ecb.AddComponent(entity, new PlayerInput
                {
                    Move = float2.zero,
                    Look = float2.zero,
                    Zoom = 0f
                });
                hasUpdated = true;
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();

        if (hasUpdated)
        {
            state.Enabled = false;
        }
    }
}