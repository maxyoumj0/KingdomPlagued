using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct BuildingBlueprintServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        //foreach (var (localTransform, blueprintOwner) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<GhostOwner>>().WithAll<BuildingBlueprintTagComponent>())
        //{
        //    // The server confirms the latest position and syncs it across all players
        //    localTransform.ValueRW.Position = SystemAPI.GetComponentRW<LocalTransform>(blueprintOwner.ValueRO).ValueRW.Position;
        //}
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
