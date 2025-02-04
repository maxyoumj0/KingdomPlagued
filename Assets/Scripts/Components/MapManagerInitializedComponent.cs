using Unity.Entities;

public struct MapManagerInitializedComponent : IComponentData
{
    public int ChunkSize;
    public int MapWidth;
    public int MapHeight;
    public float Seed;
}
