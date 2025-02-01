using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

sealed partial class DefaultVariantSystem : DefaultVariantSystemBase
{
    protected override void RegisterDefaultVariants(Dictionary<ComponentType, Rule> defaultVariants)
    {
        defaultVariants.Add(typeof(LocalTransform), Rule.OnlyParents(typeof(LocalTransformSync)));
    }
}

[GhostComponentVariation(typeof(LocalTransform), "Transform - Full Sync")]
[GhostComponent(PrefabType = GhostPrefabType.All, SendTypeOptimization = GhostSendType.AllClients)]
public struct LocalTransformSync
{
    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate)]
    public float3 Position;

    [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate)]
    public quaternion Rotation;
}