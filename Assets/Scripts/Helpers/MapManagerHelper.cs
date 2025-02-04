using Unity.Entities;
using Unity.Mathematics;

public static class MapManagerHelper
{
    public static int2 WorldToChunkCoord(float2 worldCoord, float tileSize, int chunkSize, int mapWidth, int mapHeight)
    {
        int chunkX = (int)math.floor(worldCoord.x / (tileSize * chunkSize));
        int chunkY = (int)math.floor(worldCoord.y / (tileSize / 2 * chunkSize));

        // Clamp within valid chunk bounds
        chunkX = (int)math.clamp(chunkX, 0, (mapWidth / (tileSize * chunkSize)) - 1);
        chunkY = (int)math.clamp(chunkY, 0, (mapHeight / (tileSize / 2 * chunkSize) - 1));

        return new(chunkX, chunkY);
    }

    public static int2 WorldToTileCoord(float2 worldCoord, float tileSize, int mapWidth, int mapHeight)
    {
        int tileX = (int)math.floor(worldCoord.x / tileSize);
        int tileY = (int)math.floor(worldCoord.y / tileSize);

        // TODO: Use clamp instead
        if (tileX < 0) { tileX = 0; }
        if (tileX > mapWidth) { tileX = mapWidth; }
        if (tileY < 0) { tileY = 0; }
        if (tileY > mapHeight) { tileY = mapHeight; }

        return new(tileX, tileY);
    }

    public static Entity TileTypeToPrefab(TileType tileType, PrefabReferencesComponent prefabRef)
    {
        switch (tileType)
        {
            case TileType.Grass:
                return prefabRef.GrassTilePrefab;
            case TileType.Dirt:
                return prefabRef.DirtTilePrefab;
            case TileType.Sand:
                return prefabRef.SandTilePrefab;
            case TileType.Stone:
                return prefabRef.StoneTilePrefab;
            case TileType.Water:
                return prefabRef.WaterTilePrefab;
        }
        return prefabRef.GrassTilePrefab;
    }
}
