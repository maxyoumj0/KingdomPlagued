using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MapManager : NetworkBehaviour
{
    public int MapWidth { get; set; } = 200;
    public int MapHeight { get; set; } = 100;
    public float TileRadius { get; set; } = 0.5f;
    public float Seed = 10001f;

    [Header("Tile Prefabs")]
    public GameObject P_DirtTile;
    public GameObject P_GrassTile;
    public GameObject P_WaterTile;
    public GameObject P_SandTile;
    public GameObject P_StoneTile;

    private Dictionary<TileType, GameObject> _tileTypeToPrefab;
    private TileData[,] _mapData;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //GenerateMap(); // Only the server generates the map.
        }
        else
        {
            // Client requests their initial chunk from the server.
            //RequestChunkServerRpc();
        }
    }

    void Start()
    {
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
        _mapData = new TileData[MapWidth, MapHeight];
        float a = Mathf.Sqrt(3f) * TileRadius / 2;
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            { 
                float noiseValue = Mathf.PerlinNoise((x + Seed) * 0.1f, (y + Seed) * 0.1f);
                TileType tileType = DetermineTileType(noiseValue);
                BiomeType biomeType = DetermineBiome(noiseValue);

                float CurY = y * 1.5f * TileRadius;
                float CurX = x * a * 2f;
                if (y % 2 != 0)
                {
                    CurX += a;
                }

                Vector3 worldPosition = new Vector3(CurX, 0, CurY);

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

    private void GenerateFoliage() {}

    // FIX THIS
    [ServerRpc(RequireOwnership = false)]
    public void RequestChunkServerRpc(Vector2 playerHexCoord, ServerRpcParams rpcParams = default)
    {

        // Send the relevant chunk data back to the client.
        //SendChunkToClientRpc(chunkData, rpcParams.Receive.SenderClientId);
        return;
    }

    // FIX THIS
    [ClientRpc]
    public void SendChunkToClientRpc(Vector2Int chunkData, ulong clientId)
    {
        // The server sends chunk data to the client.
        Debug.Log($"Chunk data sent to client {clientId} for chunk at {chunkData}");

        // Use chunk data to initiate tiles and objects
    }

    // Return Hex coord based on the world position
    public static Vector2Int WorldCoordToHexCoord(Vector3 worldCoord, float TileRadius, int MapWidth, int MapHeight)
    {
        float a = Mathf.Sqrt(3f) * TileRadius / 2;

        // Edge case to account for: worldCoord.z or x are negative
        int hexY = Mathf.FloorToInt(worldCoord.z / (1.5f * TileRadius));
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
            Debug.Log($"WorldCoordToHexCoord error: worldCoord out of bounds. Returning closest HexTile");
        }

        return new(hexX, hexY);
    }
}
