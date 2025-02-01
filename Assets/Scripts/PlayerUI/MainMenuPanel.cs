using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPanel : MonoBehaviour
{
    public void PlayGame()
    {
        ConnectionManager connectionManager = GameObject.FindFirstObjectByType<ConnectionManager>();
        if (connectionManager != null)
        {
            connectionManager.StartHost();
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
