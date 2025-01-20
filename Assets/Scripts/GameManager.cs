using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    public List<Vector3> spawnPoints;

    private Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();

    private void Start()
    {
        // Hook into connection approval
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            DespawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Check if this client already has a spawned player.
        if (spawnedPlayers.ContainsKey(clientId)) return;

        // Determine the spawn point for the new player.
        Vector3 spawnPoint = GetSpawnPoint(clientId);

        // Instantiate the player prefab and spawn it as the player's object.
        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint, Quaternion.identity);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId, true); // Spawn and assign ownership to the client.
            spawnedPlayers[clientId] = playerInstance; // Track the spawned player.
        }
        else
        {
            Debug.LogError("Player prefab does not have a NetworkObject component.");
        }
    }

    private void DespawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (spawnedPlayers.TryGetValue(clientId, out GameObject player))
        {
            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                networkObject.Despawn(); // Despawn the network object.
            }

            Destroy(player); // Destroy the GameObject locally.
            spawnedPlayers.Remove(clientId); // Remove it from the tracking dictionary.
        }
    }

    private Vector3 GetSpawnPoint(ulong clientId)
    {
        // Simple round-robin logic for spawn points.
        return spawnPoints[((int)clientId % spawnPoints.Count)];
    }
}