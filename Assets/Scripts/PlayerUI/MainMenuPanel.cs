using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPanel : MonoBehaviour
{
    public void PlayGame()
    {
        // TODO: Add field to create new game on the panel and take in a seed
        ConnectionManager connectionManager = GameObject.FindFirstObjectByType<ConnectionManager>();
        if (connectionManager != null)
        {
            connectionManager.NewGame(1000);
        }
    }

    public void JoinMultiplayerClient()
    {
        Debug.Log("Joining multiplayer session as client...");
        ConnectionManager connectionManager = GameObject.FindFirstObjectByType<ConnectionManager>();
        if (connectionManager != null)
        {
            connectionManager.StartClient();
        }
    }
}
