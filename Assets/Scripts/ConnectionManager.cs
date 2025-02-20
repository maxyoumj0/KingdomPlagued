using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    public string Address = "127.0.0.1";
    public ushort Port = 7979;

    // Start game creating a new save
    public void NewGame(float seed)
    {
        World server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        World client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        StartHost(server, client, seed);
    }

    // Start game using an existing save
    public void LoadGame()
    {
        // TODO: Load game using a save file
    }

    [ContextMenu("Start Host")]
    public void StartHost(World server, World client, float seed, int chunkSize = 32, int mapWidth = 200, int mapHeight = 100)
    {
        DestroyLocalWorld();
        World.DefaultGameObjectInjectionWorld ??= server;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GameScene");
        {
            using EntityQuery drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
                .Listen(ClientServerBootstrap.DefaultListenAddress.WithPort(Port));
            var entityManager = server.EntityManager;
            var seedEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(seedEntity, new PendingMapManagerSettingsComponent
            {
                ChunkSize = chunkSize,
                MapWidth = mapWidth,
                MapHeight = mapHeight,
                Seed = seed
            });
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
        World client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        DestroyLocalWorld();
        World.DefaultGameObjectInjectionWorld ??= client;

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