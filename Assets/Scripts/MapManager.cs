using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine.Tilemaps;
using Vector3 = UnityEngine.Vector3;
using System.Collections;

public class MapManager : NetworkBehaviour
{
    public int MapWidth { get; set; } = 50;
    public int MapHeight { get; set; } = 50;
    public float TileSize;
    public int ChunkSize { get; set; } = 32;
    public float Seed = 10001f;

    [Header("Tile Prefabs")]
    public NetworkObject P_DirtTile;
    public NetworkObject P_GrassTile;
    public NetworkObject P_WaterTile;
    public NetworkObject P_SandTile;
    public NetworkObject P_StoneTile;
    public NetworkObject P_DefaultTile;

    private Dictionary<TileType, NetworkObject> _tileTypeToPrefab;
    private TileData[,] _mapData;
    private HashSet<Vector2Int> _loadedChunks = new();

    public override void OnNetworkSpawn()
    {
        TileSize = P_DefaultTile.GetComponentInChildren<Renderer>().bounds.size.x;
        _tileTypeToPrefab = new()
        {
            { TileType.Dirt, P_DirtTile  },
            { TileType.Grass, P_GrassTile },
            { TileType.Water, P_WaterTile },
            { TileType.Sand, P_SandTile },
            { TileType.Stone, P_StoneTile },
        };

        foreach (var kvp in _tileTypeToPrefab)
        {
            if (kvp.Value == null)
            {
                Debug.LogError($"Prefab for {kvp.Key} is missing or not loaded. Check the Resources folder or prefab name.");
            }
        }

        if (IsServer)
        {
            if (Seed > 10000f || Seed < -10000f)
            {
                Seed = UnityEngine.Random.Range(-10000f, 10000f);
            }
        }
    }

    void Update()
    {
        
    }

    public void GenerateMap()
    {
        if (!IsServer) return;

        _mapData = new TileData[MapWidth, MapHeight];
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            { 
                float noiseValue = Mathf.PerlinNoise((x + Seed) * 0.1f, (y + Seed) * 0.1f);
                TileType tileType = DetermineTileType(noiseValue);
                BiomeType biomeType = DetermineBiome(noiseValue);

                float curY = y * TileSize * 0.5f;
                float curX = x * TileSize;
                if (y % 2 != 0)
                {
                    curX += TileSize * 0.5f;
                }

                Vector3 worldPosition = new Vector3(curX, 0, curY);

                _mapData[x, y] = new TileData
                {
                    WorldPosition = worldPosition,
                    TileType = tileType,
                    Biome = biomeType
                };
            }
        }
        Debug.Log("Map generated on server.");
    }

    private TileType DetermineTileType(float noiseValue)
    {
        if (noiseValue < 0.3f) return TileType.Water;
        if (noiseValue < 0.6f) return TileType.Sand;
        return TileType.Grass;
    }

    private BiomeType DetermineBiome(float noiseValue)
    {
        if (noiseValue < 0.3f) return BiomeType.Ocean;
        if (noiseValue < 0.6f) return BiomeType.Desert;
        return BiomeType.Plains;
    }

    // Send the relevant chunk data back to the client.
    [ServerRpc]
    public void RequestChunkServerRpc(Vector3 playerWorldPos, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        Vector3Int playerTileCoord = WorldCoordToTileCoord(playerWorldPos, TileSize, MapWidth, MapHeight);
        List<Vector2Int> chunksToLoad = new();
        // Find which tiles are in the chunk
        chunksToLoad.Add(new(
            Mathf.FloorToInt(playerTileCoord.x / (float)ChunkSize),
            Mathf.FloorToInt(playerTileCoord.z / (float)ChunkSize)
        ));

        // Add logic for finding if player is getting near the chunk border and need to spawn some neighbor chunks as well

        // Do we need to also spawn chunks that other players are in?
        chunksToLoad = chunksToLoad.Where(chunk => !_loadedChunks.Contains(chunk)).ToList();

        StartCoroutine(LoadChunks(chunksToLoad.ToArray()));
    }

    private IEnumerator LoadChunks(Vector2Int[] chunksToLoad)
    {
        int tilesSpawned = 0;
        foreach (Vector2Int chunk in chunksToLoad)
        {
            _loadedChunks.Add(chunk);
            for (int i = chunk.x * ChunkSize; i < chunk.x * ChunkSize + ChunkSize; i++)
            {
                for (int j = chunk.y * ChunkSize; j < chunk.y * ChunkSize + ChunkSize; j++)
                {
                    // This only spawns tiles. Handle entities too in the future
                    NetworkObject tileNetworkObject = _tileTypeToPrefab[_mapData[i, j].TileType];
                    NetworkObject tile = Instantiate(tileNetworkObject, _mapData[i, j].WorldPosition, tileNetworkObject.transform.rotation);
                    tile.Spawn();

                    // Looks cool 😎
                    tilesSpawned++;
                    if (tilesSpawned % 10 == 0) {
                        yield return null;
                    }
                }
            }
        }
    }

    // Return Tile coord based on the world position
    public static Vector3Int WorldCoordToTileCoord(Vector3 worldCoord, float TileSize, int MapWidth, int MapHeight)
    {
        // TODO: The camera is pointed down at an angle, so we add this offset to render chunks 
        // after moving on the z-axis when we expect to. Later on, the camera should raycast to 
        // the y=0 axis for the world coordinates.
        int zOffset = 25;

        int tileX = Mathf.FloorToInt(worldCoord.x / TileSize);
        int tileZ = Mathf.FloorToInt(worldCoord.z + zOffset / TileSize);

        if (tileX < 0) { tileX = 0; }
        if (tileX > MapWidth) { tileX = MapWidth; }
        if (tileZ < 0) { tileZ = 0; }
        if (tileZ > MapHeight) { tileZ = MapHeight; }

        return new(tileX, 0, tileZ);
    }
}
