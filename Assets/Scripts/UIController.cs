using UnityEngine;

public class UIController : MonoBehaviour
{
    public MapClickHandler clickHandler;

    public void TogglePlaceMode()
    {
        clickHandler.placeMode = !clickHandler.placeMode;

        Debug.Log("Place Mode: " + clickHandler.placeMode);
    }
}