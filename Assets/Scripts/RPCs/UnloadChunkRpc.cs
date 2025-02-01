using Unity.Mathematics;
using Unity.NetCode;

public struct UnloadChunkRpc : IRpcCommand
{
    public int2 ChunkCoord;
}
