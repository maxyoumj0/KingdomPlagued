using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    public string Address = "127.0.0.1";
    public ushort Port = 7979;

    [ContextMenu("Start Host")]
    public void StartHost()
    {
        World server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        World client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        DestroyLocalWorld();
        World.DefaultGameObjectInjectionWorld ??= server;

        SceneManager.LoadSceneAsync("GameScene");
        {
            using EntityQuery drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
                .Listen(ClientServerBootstrap.DefaultListenAddress.WithPort(Port));
        }

        {
            using EntityQuery drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
                .Connect(client.EntityManager, ClientServerBootstrap.DefaultConnectAddress.WithPort(Port));
        }
    }

    [ContextMenu("Start Client")]
    public void StartClient()
    {
        World client = ClientServerBootstrap.CreateServerWorld("ClientWorld");

        DestroyLocalWorld();

        SceneManager.LoadSceneAsync("GameScene");
        {
            using EntityQuery drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
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
