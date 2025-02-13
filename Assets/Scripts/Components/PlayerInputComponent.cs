using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class PlayerInputAuthoring : MonoBehaviour
{
    class Baker : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            AddComponent(GetEntity(TransformUsageFlags.Dynamic), new PlayerInput
            {
                Move = float2.zero,
                Look = float2.zero,
                Zoom = 0f,
                MousePos = float2.zero,
                LeftClick = 0.0f
            });
        }
    }
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    public float2 Move;
    public float2 Look;
    public float Zoom;
    public float2 MousePos;
    public float LeftClick;
}