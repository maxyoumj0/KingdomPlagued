using Unity.Entities;
using Unity.Mathematics;

public static class MapManagerHelper
{
    public static int2 WorldToChunkCoord(float2 worldCoord, int chunkSize, int mapWidth, int mapHeight)
    {
        int chunkX = (int)math.floor(worldCoord.x / chunkSize);
        int chunkY = (int)math.floor(worldCoord.y / chunkSize);

        if (chunkX < 0) { chunkX = 0; }
        if (chunkX > (float)mapWidth / chunkSize) { chunkX = mapWidth; }
        if (chunkY < 0) { chunkY = 0; }
        if (chunkY > (float)mapHeight / chunkSize) { chunkY = mapHeight; }

        return new(chunkX, chunkY);
    }

    public static int2 WorldToTileCoord(float2 worldCoord, float tileSize, int mapWidth, int mapHeight)
    {
        int tileX = (int)math.floor(worldCoord.x / tileSize);
        int tileY = (int)math.floor(worldCoord.y / tileSize);

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
