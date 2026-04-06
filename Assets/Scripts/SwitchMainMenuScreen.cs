using UnityEngine;

public class SwitchToMapMenu : MonoBehaviour
{
    public GameObject mainMenuCanvas;
    public GameObject openMapCanvas;

    void Awake()
    {
        // Sets default scene to be the main menu
        SetScreenMainMenu();
    }

    public void SetScreenMainMenu()
    {
        mainMenuCanvas.SetActive(true);
        openMapCanvas.SetActive(false);
    }

    public void SetScreenOpenMap()
    {
        mainMenuCanvas.SetActive(false);
        openMapCanvas.SetActive(true);
    }
}
