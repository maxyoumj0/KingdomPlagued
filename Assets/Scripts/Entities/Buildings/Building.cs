using Unity.Netcode;
using UnityEngine;

public class Building : Entity
{
    public int MaxCapacity { get; set; }
    public bool IsUnderConstruction { get; private set; }
    public bool IsBlueprint { get; private set; }
    public Material BlueprintMaterial;
    public Material DefaultMaterial;
    private ulong _blueprintedBy;
    private Vector3 _blueprinterMousePos;
    private MeshRenderer[] _renderers;

    public override void OnNetworkSpawn()
    {
        _renderers = GetComponentsInChildren<MeshRenderer>();
        if (_renderers == null)
        {
            Debug.Log("_renderers is null");
        }
    }

    void Update()
    {
        if (IsBlueprint && IsServer && NetworkManager.Singleton.ConnectedClients.TryGetValue(_blueprintedBy, out var networkClient))
        {
            networkClient.PlayerObject.gameObject.GetComponentInChildren<Player>().GetPlayerMousePosClientRpc(this.NetworkObjectId);
            transform.position = _blueprinterMousePos;
        }
    }

    [ServerRpc]
    public void SetBlueprinterMousePosServerRpc(Vector3 blueprinterMousePos)
    {
        _blueprinterMousePos = blueprinterMousePos;
    }

    public void ProduceUnit(Unit unitType)
    {
        // Spawn or queue a unit for production.
    }

    public void Upgrade()
    {
        // Upgrade logic for building.
    }

    [ServerRpc]
    public void SetBlueprintModeServerRpc(bool isBlueprint, ulong blueprintedBy)
    {
        IsBlueprint = isBlueprint;
        _blueprintedBy = blueprintedBy;
        foreach (MeshRenderer renderer in _renderers)
        {
            renderer.material = isBlueprint ? BlueprintMaterial : DefaultMaterial;
        }
    }
}
