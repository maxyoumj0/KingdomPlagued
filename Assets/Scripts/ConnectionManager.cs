using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public string Address;
    public ushort Port;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    [ContextMenu("Start Host")]
    void Update()
    {
        World server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        World client = ClientServerBootstrap.CreateServerWorld("ClientWorld");

        DestroyLocalWorld();
        World.DefaultGameObjectInjectionWorld ??= server;

        // SceneManager.LoadSceneAsync("Game");
        {
            using EntityQuery drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
                .Listen(ClientServerBootstrap.DefaultListenAddress.WithPort(Port));
        }

        {
            using EntityQuery drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
                .Connect(client.EntityManager, ClientServerBootstrap.DefaultConnectAddress.WithPort(Port));
        }
    }

    private void DestroyLocalWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }
}
