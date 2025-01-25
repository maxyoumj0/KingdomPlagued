using UnityEngine;

public class InteractPanel : MonoBehaviour
{
    public GameObject BuildPanel;
    public GameObject TestBuilding;

    public void BuildButton()
    {
        Debug.Log("BuildButton Clicked");
        BuildPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
