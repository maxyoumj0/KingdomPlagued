using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct BuildingPlacementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton(out MapDataComponent mapData))
            return;
        if (!SystemAPI.TryGetSingleton(out BuildingPrefabComponent prefabRefs))
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (buildingSelectedRpc, receiveRpcCommand, entity) in SystemAPI.Query<RefRO<BuildingSelectedRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            Debug.Log("Received BuildingSelectedRpc");
            Entity buildingBlueprintPrefabEntity = BuildingPrefabHelper.BuildingEnumToEntity(prefabRefs, buildingSelectedRpc.ValueRO.BuildingEnum, true);
            int senderNetworkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommand.ValueRO.SourceConnection).Value;

            Entity buildingBlueprintEntity = ecb.Instantiate(buildingBlueprintPrefabEntity);
            ecb.SetComponent(buildingBlueprintEntity, new LocalTransform
            {
                Position = new float3(0, 0, 0),
                Rotation = SystemAPI.GetComponent<LocalTransform>(buildingBlueprintPrefabEntity).Rotation,
                Scale = SystemAPI.GetComponent<LocalTransform>(buildingBlueprintPrefabEntity).Scale
            });
            ecb.SetComponent(buildingBlueprintEntity, new GhostOwner
            {
                NetworkId = senderNetworkId
            });

            ecb.DestroyEntity(entity);
        }

        foreach (var (buildingPlacedCommand, entity) in SystemAPI.Query<RefRO<BuildingPlacedComponent>>().WithEntityAccess())
        {
            int networkId = buildingPlacedCommand.ValueRO.NetworkId;
            BuildingEnum buildingEnum = BuildingEnum.TestBuilding;
            foreach (var (ghostOwner, blueprintTag, blueprintEntity) in SystemAPI.Query<RefRO<GhostOwner>, RefRO<BuildingBlueprintComponent>>().WithEntityAccess())
            {
                if (ghostOwner.ValueRO.NetworkId == networkId)
                {
                    buildingEnum = blueprintTag.ValueRO.buildingEnum;
                    ecb.DestroyEntity(blueprintEntity);
                    break;
                }
            }

            LocalTransform transform = buildingPlacedCommand.ValueRO.LocalTransform;
            Entity buildingPrefabEntity = BuildingPrefabHelper.BuildingEnumToEntity(prefabRefs, buildingEnum, false);
            Entity buildingEntity = ecb.Instantiate(buildingPrefabEntity);
            ecb.SetComponent(buildingEntity, transform);
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
