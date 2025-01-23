using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : NetworkBehaviour
{
    public NetworkObject playerPrefab;
    public Tuple<Vector3, Quaternion> spawnPoint;

    private static GameManager _instance;
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
        spawnPoint = new(
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

        NetworkObject playerInstance = Instantiate(playerPrefab, spawnPoint.Item1, spawnPoint.Item2);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId, true);
            // change later to a real coord
            playerInstance.GetComponent<Player>().SetPlayerHexTileCoord(0, 0);
            _players[clientId] = playerInstance;
        }
        else
        {
            Debug.LogError("Player prefab does not have a NetworkObject component.");
        }
    }
}