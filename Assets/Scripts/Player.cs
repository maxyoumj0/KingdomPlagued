using System;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor { get; set; } = new NetworkVariable<Color>(Color.blue);
    public NetworkList<NetworkObjectReference> SelectedEntities = new();
    public GameObject PlayerCamera;
    private Tuple<int, int> _playerHexTileCoord = new(0, 0);

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
        if (transform.hasChanged && CheckPlayerMovedHex())
        {
            
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
    public void SetPlayerHexTileCoord(int x, int y)
    {
        _playerHexTileCoord = new(x, y);
    }

    private bool CheckPlayerMovedHex()
    {

    }

    [ServerRpc]
    public void UpdatePlayerColorServerRpc(Color newColor)
    {
        PlayerColor.Value = newColor; // Update the player color on the server
    }
}