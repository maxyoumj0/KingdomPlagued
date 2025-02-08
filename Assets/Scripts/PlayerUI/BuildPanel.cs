using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

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
        clientWorld = World.DefaultGameObjectInjectionWorld;
        entityManager = clientWorld.EntityManager;
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
        var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<NetworkId>());
        if (query.IsEmpty)
        {
            Debug.LogError("No client connection found!");
            return;
        }
        Entity networkIdEntity = query.GetSingletonEntity();
        Entity rpcEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(rpcEntity, new BuildingSelectedRpc { BuildingEnum = buildingEnum });
        entityManager.AddComponentData(rpcEntity, new SendRpcCommandRequest { TargetConnection = networkIdEntity });
    }
}
