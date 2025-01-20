using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        NetworkManager.Singleton.StartHost();
    }

    public void JoinMultiplayerClient()
    {
        Debug.Log("Joining multiplayer session as client...");
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
