using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> CursorLocation;
    public NetworkVariable<Color> PlayerColor;
    public NetworkVariable<int> PlayerID;

    public GameObject PlayerCamera;

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
        
    }

    [ServerRpc]
    public void UpdatePlayerColorServerRpc(Color newColor)
    {
        PlayerColor.Value = newColor; // Update the player color on the server
    }
}