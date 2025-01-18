using UnityEngine;
using Unity.Netcode;

public class Entity : NetworkBehaviour
{
    // Network variable to track selection status
    public NetworkVariable<bool> IsSelected = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private GameObject _selectionMarker;

    private void Awake()
    {
        // Find the selection marker in the hierarchy
        _selectionMarker = transform.Find("P_SelectionMarker")?.gameObject;

        if (_selectionMarker != null)
        {
            _selectionMarker.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        IsSelected.OnValueChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(bool oldValue, bool newValue)
    {
        if (_selectionMarker != null)
        {
            _selectionMarker.SetActive(newValue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSelectedServerRpc(bool selected)
    {
        // Only the server updates the NetworkVariable
        IsSelected.Value = selected;
    }
}