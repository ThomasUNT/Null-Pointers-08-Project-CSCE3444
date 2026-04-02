using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D cursorTexture; // Assign your "Cursor" type texture here
    public Vector2 hotSpot = Vector2.zero; // The clickable point (0,0 is top-left)
    public CursorMode cursorMode = CursorMode.Auto;

    void Start()
    {
        // Set the custom cursor when the game starts
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    // Example: Change cursor on hover (requires a Collider and this script on the target)
    void OnMouseEnter()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    void OnMouseExit()
    {
        // Reset to default
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}