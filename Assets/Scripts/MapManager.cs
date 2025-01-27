using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MapManager : NetworkBehaviour
{
    public int MapWidth { get; set; } = 50;
    public int MapHeight { get; set; } = 50;
    public float TileSize { get; set; }
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
    [ServerRpc(RequireOwnership = false)]
    public void RequestChunkServerRpc(Vector3 playerHexCoord, ServerRpcParams rpcParams = default)
    {
        Vector2Int playerTileCoord = WorldCoordToTileCoord(playerHexCoord, TileSize, MapWidth, MapHeight);
        
        // Find which tiles are in the chunk
        Vector2Int chunk = new(
            Mathf.FloorToInt(playerTileCoord.x / (float)ChunkSize),
            Mathf.FloorToInt(playerTileCoord.y / (float ) ChunkSize)
        );
        SendChunkToClientRpc(chunk, rpcParams.Receive.SenderClientId);
    }

    // FIX THIS
    [ClientRpc]
    public void SendChunkToClientRpc(Vector2Int chunkData, ulong clientId)
    {
        // The server sends chunk data to the client.
        Debug.Log($"Chunk data sent to client {clientId} for chunk at {chunkData}");

        // Use chunk data to initiate tiles and objects
        for (int i = chunkData.x * ChunkSize; i < chunkData.x * ChunkSize + ChunkSize - 1; i++)
        {
            for (int j = chunkData.y * ChunkSize; i < chunkData.y * ChunkSize + ChunkSize - 1; i++)
            {
                NetworkObject tileNetworkObject = _tileTypeToPrefab[_mapData[i, j].TileType];
                NetworkObject tile = Instantiate(tileNetworkObject, _mapData[i, j].WorldPosition, tileNetworkObject.transform.rotation);
                tile.Spawn();
            }
        }
    }

    // Return Hex coord based on the world position
    public static Vector2Int WorldCoordToTileCoord(Vector3 worldCoord, float TileSize, int MapWidth, int MapHeight)
    {
        float a = Mathf.Sqrt(3f) * TileSize / 2;

        // Edge case to account for: worldCoord.z or x are negative
        int hexY = Mathf.FloorToInt(worldCoord.z / (1.5f * TileSize));
        float xOffset = (hexY % 2 != 0) ? a : 0;
        int hexX = Mathf.FloorToInt((worldCoord.x - xOffset) / (2f * a));

        // check out of bounds
        bool outOfBounds = false;
        if (worldCoord.x < (-1 * a))
        {
            outOfBounds = true;
            hexX = 0;
        }
        else if (worldCoord.x > ((MapWidth - 1) * a + a))
        {
            outOfBounds = true;
            hexX = MapWidth;
        }
        else if (worldCoord.z < (-1 * a))
        {
            outOfBounds = true;
            hexY = 0;
        }
        else if (worldCoord.z > (MapHeight - 1) * a + a)
        {
            outOfBounds = true;
            hexY = MapHeight;
        }
        if (outOfBounds)
        {
            Debug.Log($"WorldCoordToTileCoord error: worldCoord out of bounds. Returning closest HexTile");
        }

        return new(hexX, hexY);
    }
}
