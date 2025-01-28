using System;
using System.Numerics;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Vector3 = UnityEngine.Vector3;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Color> PlayerColor { get; set; } = new NetworkVariable<Color>(Color.blue);
    public NetworkList<NetworkObjectReference> SelectedEntities = new();
    public GameObject PlayerCamera;

    private Vector2Int _playerTileCoord = new(0, 0);
    private int _mapHeight;
    private int _mapWidth;
    private float _tileRadius;
    private MapManager _mapManager;
    private bool _placingBuilding = false;
    private Building _buildingBeingPlaced;
    private InputAction _leftClickAction;
    private Vector3 _lastPlayerPos;
    private float _posChangeThreshold = 0.5f;

    private void Awake()
    {
        PlayerCamera = transform.Find("P_PlayerCamera")?.gameObject;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _lastPlayerPos = transform.position;
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
        _leftClickAction = InputSystem.actions.FindAction("LeftClick");
    }

    private void Update()
    {
        if (!IsClient) return;

        // Request for chunks when Player moved to a different tile (Modify this to chunk later)
        Vector3 offset = _lastPlayerPos - transform.position;
        if ((offset.x > _posChangeThreshold || offset.z > _posChangeThreshold) && PlayerMovedTile())
        {
            _mapManager.RequestChunkServerRpc(transform.position);
        }

        if (_placingBuilding)
        {
            HandleLeftClickOnBuild();
        }
    }

    private void HandleLeftClickOnBuild()
    {
        if (_leftClickAction.IsPressed())
        {
            _buildingBeingPlaced.SetBlueprintModeServerRpc(false, this.NetworkObjectId);
        }
    }

    [ClientRpc]
    public void InitializePlayerClientRpc(int mapHeight, int mapWidth, float tileRadius, Vector2Int spawnPointTileCoord)
    {
        _mapHeight = mapHeight;
        _mapWidth = mapWidth;
        _tileRadius = tileRadius;
        _playerTileCoord = spawnPointTileCoord;
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

    private bool PlayerMovedTile()
    {
        if (_mapManager != null)
        {
            Vector2Int convertedTileCoord = MapManager.WorldCoordToTileCoord(transform.position, _tileRadius, _mapWidth, _mapHeight);
            if (!_playerTileCoord.Equals(convertedTileCoord))
            {
                _playerTileCoord = convertedTileCoord;
                return true;
            }
        }
        return false;
    }

    [ClientRpc]
    public void GetPlayerMousePosClientRpc(ulong networkObjectID)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectID, out NetworkObject entityNetworkObject))
        {
            Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Mouse.current.position.ReadValue());
            Building building = entityNetworkObject.GetComponent<Building>();
            if (building != null && Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Tile"))
            {
                building.SetBlueprinterMousePosServerRpc(hit.point);
                _placingBuilding = true;
                _buildingBeingPlaced = building;
            }
        }
    }
}