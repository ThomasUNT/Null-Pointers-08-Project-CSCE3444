using UnityEngine;

public class DrawHandler : MonoBehaviour
{
    public bool landMode = false;
    public bool waterMode = false;

    public void ToggleLandMode()
    {
        landMode = !landMode;
        waterMode = false; // Ensure water mode is off when land mode is toggled on

        Debug.Log("Land Mode: " + landMode);
    }

    public void ToggleWaterMode()
    {
        waterMode = !waterMode;
        landMode = false; // Ensure land mode is off when water mode is toggled on

        Debug.Log("Water Mode: " + waterMode);
    }
}