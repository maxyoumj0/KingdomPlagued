using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class BuildPanel : MonoBehaviour
{
    public GameObject InteractPanel;
    private World clientWorld;
    private EntityManager entityManager;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        foreach(var world in World.All)
        {
            if (world.IsClient() && !world.IsThinClient())
            {
                clientWorld = world;
                entityManager = clientWorld.EntityManager;
                break;
            }
        }
    }

    public void TestBuilding()
    {
        SendBuildingRpc(BuildingEnum.TestBuilding);
    }

    public void Back()
    {
        InteractPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void SendBuildingRpc(BuildingEnum buildingEnum)
    {
        Debug.Log("SendBuildingRpc called");
        if (clientWorld == null || !clientWorld.IsCreated) return;

        var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<NetworkId>(), ComponentType.ReadOnly<NetworkStreamConnection>());
        if (query.IsEmpty)
        {
            Debug.LogError("No client connection found!");
            return;
        }
        Entity networkIdEntity = query.GetSingletonEntity();
        Entity rpcEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(rpcEntity, new BuildingSelectedRpc { BuildingEnum = buildingEnum });
        entityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = networkIdEntity });
        Debug.Log($"Sending RPC from world: {clientWorld.Name}");
    }
}
