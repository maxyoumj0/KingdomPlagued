using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp).WithAll<ReceiveRpcCommandRequest>().WithAll<GoInGameRequestRpc>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<PrefabReferencesComponent>(out Entity prefabRefEntity))
            return;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest, Entity RpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            ecb.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            int networkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection).Value;
            PrefabReferencesComponent playerPrefabEntity = SystemAPI.GetComponent<PrefabReferencesComponent>(prefabRefEntity);
            
            Entity playerEntity = ecb.Instantiate(playerPrefabEntity.PlayerPrefab);
            // Set Spawnpoint
            ecb.SetComponent(playerEntity, new LocalTransform
            {
                Position = new float3(0, 10.0f, 0),
                Rotation = SystemAPI.GetComponent<LocalTransform>(playerPrefabEntity.PlayerPrefab).Rotation,
                Scale = SystemAPI.GetComponent<LocalTransform>(playerPrefabEntity.PlayerPrefab).Scale
            });
            // Set PlayerComponent
            PlayerComponent playerComponent = new PlayerComponent
            {
                OwnerNetworkId = networkId
            };
            // Set GhostId
            ecb.SetComponent(playerEntity, playerComponent);
            ecb.SetComponent(playerEntity, new GhostOwner { NetworkId = networkId });

            ecb.DestroyEntity(RpcEntity);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
