using System;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor { get; set; } = new NetworkVariable<Color>(Color.blue);
    public NetworkList<NetworkObjectReference> SelectedEntities = new();
    public GameObject PlayerCamera;

    private Vector2Int _playerHexTileCoord = new(0, 0);
    private int _mapHeight;
    private int _mapWidth;
    private float _tileRadius;
    private MapManager _mapManager;

    private void Awake()
    {
        PlayerCamera = transform.Find("P_PlayerCamera")?.gameObject;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            // Enable the player's camera if this is the local player
            PlayerCamera.SetActive(true);
        }
        else
        {
            // Disable the camera for non-local players
            PlayerCamera.SetActive(false);
        }
    }

    private void Update()
    {
        // Request for chunks when Player moved to a different hex (Modify this to chunk later)
        if (transform.hasChanged && PlayerMovedHex())
        {
            _mapManager.RequestChunkServerRpc(
                new Vector2(
                    _playerHexTileCoord.x,
                    _playerHexTileCoord.y
                )
            );
        }
    }

    [ClientRpc]
    public void InitializePlayerClientRpc(int mapHeight, int mapWidth, float tileRadius, Vector2Int spawnPointHexCoord)
    {
        _mapHeight = mapHeight;
        _mapWidth = mapWidth;
        _tileRadius = tileRadius;
        _playerHexTileCoord = spawnPointHexCoord;
    }

    public void AddToSelection(NetworkObjectReference entity)
    {
        if (!SelectedEntities.Contains(entity))
        {
            SelectedEntities.Add(entity);
        }
    }

    public void RemoveFromSelection(NetworkObjectReference entity)
    {
        if (SelectedEntities.Contains(entity))
        {
            SelectedEntities.Remove(entity);
        }
    }

    [ClientRpc]
    public void RemoveFromSelectionClientRpc(NetworkObjectReference entity)
    {
        if (SelectedEntities.Contains(entity))
        {
            SelectedEntities.Remove(entity);
        }
    }

    public void ClearSelection()
    {
        SelectedEntities.Clear();
    }

    private bool PlayerMovedHex()
    {
        if (_mapManager != null)
        {
            Vector2Int convertedHexCoord = MapManager.WorldCoordToHexCoord(transform.position, _tileRadius, _mapWidth, _mapHeight);
            if (!_playerHexTileCoord.Equals(convertedHexCoord))
            {
                _playerHexTileCoord = convertedHexCoord;
                return true;
            }
        }
        return false;
    }
}