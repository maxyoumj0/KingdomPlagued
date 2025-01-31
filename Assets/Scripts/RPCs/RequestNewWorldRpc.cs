using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[BurstCompile]
public struct RequestNewWorldRpc : IRpcCommand
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}
