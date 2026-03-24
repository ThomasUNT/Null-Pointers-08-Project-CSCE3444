using UnityEngine;

public class UIController : MonoBehaviour
{
    public MapClickHandler clickHandler;
    public MapSettingsPanel mapSettingsPanel;

    public void TogglePlaceMode()
    {
        clickHandler.placeMode = !clickHandler.placeMode;

        Debug.Log("Place Mode: " + clickHandler.placeMode);
    }

    public void ToggleTextPlaceMode()
    {
        clickHandler.textPlaceMode = !clickHandler.textPlaceMode;

        Debug.Log("Text Place Mode: " + clickHandler.textPlaceMode);
    }
    public void OpenMapSettings(){
      mapSettingPanel.OpenMapSettings();
    }
}
