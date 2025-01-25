using UnityEngine;

public class InteractPanel : MonoBehaviour
{
    public GameObject BuildPanel;

    public void BuildButton()
    {
        gameObject.SetActive(false);
        BuildPanel.SetActive(true);
    }
}
