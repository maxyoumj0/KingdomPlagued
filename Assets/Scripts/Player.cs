using System;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor { get; set; } = new NetworkVariable<Color>(Color.blue);
    public NetworkList<NetworkObjectReference> SelectedEntities = new();
    public GameObject PlayerCamera;
    private Tuple<int, int> _playerHexTileCoord = new(0, 0);

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

        _mapManager = GameObject.Find("P_MapManger").GetComponent<MapManager>();
    }

    private void Update()
    {
        if (transform.hasChanged && PlayerMovedHex())
        {
            _mapManager.RequestChunkServerRpc(
                new Vector2(
                    _playerHexTileCoord.Item1,
                    _playerHexTileCoord.Item2
                )
            );
        }
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

    [ClientRpc]
    public void SetPlayerHexTileCoordClientRpc(int x, int y)
    {
        _playerHexTileCoord = new(x, y);
    }

    private bool PlayerMovedHex()
    {
        Tuple<int, int> convertedHexCoord = MapManager.WorldCoordToHexCoord(transform.position, _mapManager.TileRadius, _mapManager.MapWidth, _mapManager.MapHeight);
        if (!_playerHexTileCoord.Equals(convertedHexCoord))
        {
            _playerHexTileCoord = convertedHexCoord;
            return true;
        }
        return false;
    }
}