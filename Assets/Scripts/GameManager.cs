using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : NetworkBehaviour
{
    public NetworkObject PlayerPrefab;
    public NetworkObject MapManagerPrefab;
    // Implement function for setting `SpawnPoint`
    public Tuple<Vector3, Quaternion> SpawnPoint;

    [Header("Map Settings")]
    public int MapWidth = 200;
    public int MapHeight = 100;
    public float TileRadius = 0.5f;

    private static GameManager _instance;
    private NetworkObject _mapManager;
    private Dictionary<ulong, NetworkObject> _players = new();
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Creating testing spawnPoint automate populating `spawnPoints` later
        SpawnPoint = new(
            new Vector3(0, 10, -20),
            Quaternion.identity
        );
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public void StartHost()
    {

        StartCoroutine(StartHostWithSceneLoad());
    }

    public void StartClient(string ipAddress)
    {
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.SetConnectionData(ipAddress, 7777);
        NetworkManager.Singleton.StartClient();
    }

    private IEnumerator StartHostWithSceneLoad()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

        // Wait until the scene has fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Instantiate MapManager NetworkObject and generate the map
        _mapManager = Instantiate(MapManagerPrefab, Vector3.zero, Quaternion.identity);
        MapManager mapManagerComponent = _mapManager.GetComponent<MapManager>();
        mapManagerComponent.MapHeight = MapHeight;
        mapManagerComponent.MapWidth = MapWidth;
        mapManagerComponent.GenerateMap();

        NetworkManager.Singleton.StartHost();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            _players.Remove(clientId);
        }
    }
    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;

        if (_players.ContainsKey(clientId)) return;

        NetworkObject playerInstance = Instantiate(PlayerPrefab, SpawnPoint.Item1, SpawnPoint.Item2);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId, true);
            // change later to a real coord
            playerInstance.GetComponent<Player>().InitializePlayerClientRpc(MapHeight, MapWidth, TileRadius, Vector2Int.zero);
            _players[clientId] = playerInstance;
        }
        else
        {
            Debug.LogError("Player prefab does not have a NetworkObject component.");
        }
    }
}