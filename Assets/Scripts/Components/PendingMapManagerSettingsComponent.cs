using Unity.Entities;

public struct PendingMapManagerSettingsComponent : IComponentData
{
    public int ChunkSize;
    public int MapWidth;
    public int MapHeight;
    public float Seed;
}
