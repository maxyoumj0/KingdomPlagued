using Unity.Entities;
using Unity.NetCode;

public struct GenClientMapRpc : IRpcCommand
{
    public float Seed;
}
