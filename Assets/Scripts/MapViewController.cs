using UnityEngine;
using TMPro;

public class MapViewController : MonoBehaviour
{
    public GameObject buttonPanel;
    public GameObject mapPanel;
    public GameObject fullscreenPanel;
    
    public void OpenFullscreenPanel()
    {
        buttonPanel.SetActive(false);
        mapPanel.SetActive(false);
        fullscreenPanel.SetActive(true);
    }

    public void CloseFullscreenPanel()
    {
        buttonPanel.SetActive(true);
        mapPanel.SetActive(true);
        fullscreenPanel.SetActive(false);
    }

}
