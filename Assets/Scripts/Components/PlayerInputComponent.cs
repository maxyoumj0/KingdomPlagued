using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    public float2 Move;
    public float2 Look;
    public float Zoom;
}