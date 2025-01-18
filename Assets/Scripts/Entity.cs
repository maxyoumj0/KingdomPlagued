using UnityEngine;
using Unity.Netcode;

public class Entity : NetworkBehaviour
{
    public NetworkVariable<bool> IsSelected = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // add player network variable
    private NetworkVariable<GameObject> _selectionMarker;

    private void Awake()
    {
        // Find the selection marker in the hierarchy
        _selectionMarker = new(transform.Find("P_SelectionMarker")?.gameObject);
        if (_selectionMarker != null)
        {
            _selectionMarker.Value.SetActive(false);
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
            _selectionMarker.Value.SetActive(newValue);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSelectedServerRpc(bool selected) // add another parameter for player
    {
        IsSelected.Value = selected;
    }
}