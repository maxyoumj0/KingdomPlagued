using UnityEngine;
using UnityEngine.InputSystem;

public class BuildPanel : MonoBehaviour
{
    public GameObject InteractPanel;
    private GameObject _localPlayer;
    void Awake()
    {
        gameObject.SetActive(false);
    }

    void Start()
    {
        //if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        //{
        //    // Get the local player's NetworkObject
        //    var localPlayerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        //    if (localPlayerObject != null)
        //    {
        //        _localPlayer = localPlayerObject.gameObject;
        //    }
        //    else
        //    {
        //        Debug.LogError("Local player object not found!");
        //    }
        //}
        //else
        //{
        //    Debug.LogError("NetworkManager not found or not running as a client!");
        //}
    }

    public void TestBuilding()
    {
        //GameManager.Instance.SpawnBuildingBlueprintServerRpc(BuildingEnum.TestBuilding, Mouse.current.position.ReadValue());
    }

    public void Back()
    {
        InteractPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
