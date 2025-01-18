using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : NetworkBehaviour
{
    private NetworkManager m_NetworkManager;

    private Dictionary<ulong, Player> _players = new();

    void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
        if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
        if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
    }

    void StatusLabels()
    {
        var mode = m_NetworkManager.IsHost ?
            "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
    }

    public void RegisterPlayer(ulong clientId, Player player)
    {
        if (!_players.ContainsKey(clientId))
        {
            _players.Add(clientId, player);
        }
    }

    public void UnregisterPlayer(ulong clientId)
    {
        if (_players.ContainsKey(clientId))
        {
            _players.Remove(clientId);
        }
    }

    public Player GetPlayer(ulong clientId)
    {
        return _players.TryGetValue(clientId, out var player) ? player : null;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        UnregisterPlayer(clientId);
    }
}