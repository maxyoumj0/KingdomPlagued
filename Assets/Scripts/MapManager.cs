using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class MapManager : NetworkBehaviour
{
    [Header("Map Settings")]
    public int MapWidth = 200;
    public int MapHeight = 100;
    public float TileRadius = 0.5f;
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
            RequestInitialChunksServerRpc();
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
            GenerateMap();
        }
    }

    void Update()
    {
        
    }

    private void GenerateMap()
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

    [ServerRpc(RequireOwnership = false)]
    private void RequestInitialChunksServerRpc(ServerRpcParams rpcParams = default)
    {
        // Determine the chunk based on `SpawnPoint`
        Vector2Int chunkData = SpawnPoint;

        // Calculate the initial chunk to be loaded

        // Send the relevant chunk data back to the client.
        SendChunkToClientRpc(chunkData, rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void SendChunkToClientRpc(Vector2Int chunkData, ulong clientId)
    {
        // The server sends chunk data to the client.
        Debug.Log($"Chunk data sent to client {clientId} for chunk at {chunkData}");

        // Use chunk data to initiate tiles and objects
    }

    // Return Hex coord based on the world position
    public static Tuple<int, int> WorldCoordToHexCoord(Vector3 worldCoord)
    {
        // maybe this function shouldn't be static? Mind the hard coded `TileRadius`
        float TileRadius = 0.5f;
        float a = Mathf.Sqrt(3f) * TileRadius / 2;

        int hexX = ;

        return new Tuple<int, int>(hexX, hexY);
    }
}
