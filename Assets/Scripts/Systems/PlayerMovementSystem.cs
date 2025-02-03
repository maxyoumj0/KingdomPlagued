using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct PlayerMovementSystem : ISystem
{
    // TODO: Should be received from player setting
    private const float MoveSpeed = 10.0f;
    private const float ZoomSpeed = 50.0f;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerInput>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        foreach ((RefRO<PlayerInput> playerInput, RefRW<LocalTransform> localTransform) in SystemAPI.Query<RefRO<PlayerInput>, RefRW<LocalTransform>>().WithAll<Simulate>())
        {
            Vector2 moveInput = playerInput.ValueRO.Move;
            float3 moveDirection = new float3(moveInput.x, 0, moveInput.y) * MoveSpeed * deltaTime;
            localTransform.ValueRW.Position += moveDirection;
            float zoomInput = playerInput.ValueRO.Zoom;
            //Debug.Log($"zoomInput: {zoomInput}");
            localTransform.ValueRW.Position += new float3(0, zoomInput, 0) * ZoomSpeed * deltaTime;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
