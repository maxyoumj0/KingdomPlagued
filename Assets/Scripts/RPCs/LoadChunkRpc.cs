using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct LoadChunkRpc : IRpcCommand
{
    public int2 ChunkCoord;
}
