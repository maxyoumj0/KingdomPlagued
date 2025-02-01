using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

partial struct PlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //foreach (var (player, transform, inputBuffer, entity) in
        //         SystemAPI.Query<RefRW<PlayerComponent>, RefRW<LocalTransform>, InputBuffer<PlayerInput>>()
        //             .WithEntityAccess())
        //{
        //    if (!inputBuffer.TryGetDataAtTick(SystemAPI.GetSingleton<NetworkTime>().ServerTick, out var input))
        //        continue; // Skip if no input

        //    float3 move = new float3(input.MoveDirection.x, 0, input.MoveDirection.y);
        //    transform.ValueRW.Position += move * SystemAPI.Time.DeltaTime;
        //}
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
