using UnityEngine;

public class MapDrawHandler : MonoBehaviour
{
    public RectTransform mapRect;
    public MapMaskManager maskManager;

    public bool landMode = false;
    public bool waterMode = false;

    void Update()
    {
        if (!landMode && !waterMode) return;

        if (Input.GetMouseButton(0))
        {
            HandleDraw(Input.mousePosition);
        }
    }

    private void HandleDraw(Vector2 screenPosition)
    {
        if (!TryGetLocalPoint(screenPosition, out Vector2 localPoint))
            return;

        if (!TryGetNormalizedPoint(localPoint, out Vector2 normalizedPoint))
            return;

        DrawPixel(normalizedPoint);
    }

    private bool TryGetLocalPoint(Vector2 screenPosition, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRect,
            screenPosition,
            null,
            out localPoint
        );
    }

    private bool TryGetNormalizedPoint(Vector2 localPoint, out Vector2 normalizedPoint)
    {
        Rect rect = mapRect.rect;

        float normalizedX = (localPoint.x - rect.x) / rect.width;
        float normalizedY = (localPoint.y - rect.y) / rect.height;

        normalizedPoint = new Vector2(normalizedX, normalizedY);

        return normalizedX >= 0f && normalizedX <= 1f &&
               normalizedY >= 0f && normalizedY <= 1f;
    }

    private void DrawPixel(Vector2 normalizedPoint)
    {
        Texture2D tex = maskManager.maskTexture;

        int px = Mathf.FloorToInt(normalizedPoint.x * tex.width);
        int py = Mathf.FloorToInt(normalizedPoint.y * tex.height);

        if (px < 0 || px >= tex.width || py < 0 || py >= tex.height)
            return;

        Color32 color = Color.white;

        if (landMode)
            color = Color.black;
        else if (waterMode)
            color = Color.white;

        tex.SetPixel(px, py, color);
        tex.Apply();
    }

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