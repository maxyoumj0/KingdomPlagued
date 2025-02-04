using Unity.Entities;
using Unity.NetCode;

public struct SendMapManagerSettingsRpc : IRpcCommand
{
    public int ChunkSize;
    public float TileSize;
    public int MapWidth;
    public int MapHeight;
    public float Seed;
}
