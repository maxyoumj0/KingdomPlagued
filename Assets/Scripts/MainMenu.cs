using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        GameManager gameManager = GameObject.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartHost();
        }
    }

    public void JoinMultiplayerClient()
    {
        Debug.Log("Joining multiplayer session as client...");
        SceneManager.LoadScene("GameScene");
        GameObject.FindFirstObjectByType<GameManager>().StartClient("127.0.0.1");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
