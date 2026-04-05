using UnityEngine;

public class MapDrawHandler : MonoBehaviour
{
    public RectTransform mapRect;
    public MapMaskManager maskManager;

    [Range(1, 50)]
    public int brushSize = 4;

    public bool landMode = false;
    public bool waterMode = false;
    public bool forestMode = false;
    public bool mountainMode = false;
    public bool tundraMode = false;
    public bool desertMode = false;

    private Vector2? lastPixelPos = null;

    void Update()
    {
        // Do nothing if neither mode is active
        if (!landMode && !waterMode && !forestMode && !mountainMode && !tundraMode && !desertMode)
            return;

        // When mouse is released, stop connecting pixels
        if (Input.GetMouseButtonUp(0))
        {
            lastPixelPos = null;
        }

        // While mouse is held, continuously draw and connect pixels
        if (Input.GetMouseButton(0) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
        {
            HandleDraw(Input.mousePosition);

            maskManager.maskTexture.Apply(); // Apply changes to texture after drawing
        }
    }

    private void HandleDraw(Vector2 screenPosition)
    {
        // Convert Screen position to Local UI space
        if (!TryGetLocalPoint(screenPosition, out Vector2 localPoint))
        {
            lastPixelPos = null;
            return;
        }

        // Convert Local point to normalized 0 - 1 range
        if (!TryGetNormalizedPoint(localPoint, out Vector2 normalizedPoint))
        {
            lastPixelPos = null;
            return;
        }

        // Draw on texture
        DrawPixel(normalizedPoint);
    }

    public void SetBrushSize(float value)
    {
        brushSize = Mathf.RoundToInt(value);
    }

    // Converts screen position to local position within the mapRect
    private bool TryGetLocalPoint(Vector2 screenPosition, out Vector2 localPoint)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRect,
            screenPosition,
            null,
            out localPoint
        );
    }

    // Converts local UI coordinates into normalized 0-1 texture coordinates
    // Returns false if outside bounds of map
    private bool TryGetNormalizedPoint(Vector2 localPoint, out Vector2 normalizedPoint)
    {
        Rect rect = mapRect.rect;

        float normalizedX = (localPoint.x - rect.x) / rect.width;
        float normalizedY = (localPoint.y - rect.y) / rect.height;

        normalizedPoint = new Vector2(normalizedX, normalizedY);

        // Ensure the point lies within the drawable area
        return normalizedX >= 0f && normalizedX <= 1f &&
               normalizedY >= 0f && normalizedY <= 1f;
    }

    private void DrawPixel(Vector2 normalizedPoint)
    {
        Texture2D tex = maskManager.maskTexture;

        // Convert normalized coordinates to pixel coordinates
        int px = Mathf.FloorToInt(normalizedPoint.x * tex.width);
        int py = Mathf.FloorToInt(normalizedPoint.y * tex.height);

        // Check if pixel coordinates are within texture bounds
        if (px < 0 || px >= tex.width || py < 0 || py >= tex.height)
            return;

        Vector2 current = new Vector2(px, py);
        
        // Determine color based on mode
        Color32 color = Color.white;

        Color32[] pixels = tex.GetPixels32();
        int width = tex.width;
        int height = tex.height;

        if (landMode)
            color = Color.black;
        else if (waterMode)
            color = Color.white;
        else if (forestMode)
            color = Color.green;
        else if (mountainMode)
            color = Color.red;
        else if (tundraMode)
            color = Color.gray;
        else if (desertMode)
            color = Color.yellow;

        // If we have previous pixel, draw a line to avoid gaps
        if (lastPixelPos.HasValue)
        {
            DrawLine(pixels, width, height, lastPixelPos.Value, current, color);
        }
        else
        {
            DrawBrush(pixels, width, height, px, py, color);
        }

        // Apply modified pixels back to texture
        tex.SetPixels32(pixels);

        // Store current pixel as last pixel for next frame
        lastPixelPos = current;
    }

    // Draws a line between two points
    void DrawLine(Color32[] pixels, int width, int height, Vector2 from, Vector2 to, Color32 color)
    {
        // Number of steps based on distance between points
        int steps = Mathf.CeilToInt(Vector2.Distance(from, to));

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;

            // Interpolate between start and end points
            int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));

            DrawBrush(pixels, width, height, x, y, color);
        }
    }

    void DrawBrush(Color32[] pixels, int width, int height, int cx, int cy, Color32 color)
    {
        int radiusSq = brushSize * brushSize;

        for (int x = -brushSize; x <= brushSize; x++)
        {
            for (int y = -brushSize; y <= brushSize; y++)
            {
                if (x * x + y * y > radiusSq)
                    continue; // Skip pixels outside the circular brush

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || px >= width || py < 0 || py >= height)
                    continue; // Skip pixels outside texture bounds

                int index = py * width + px;

                if (pixels[index].Equals(color))
                    continue; // Skip if pixel already has the target color

                pixels[index] = color;
            }
        }
    }

    public void ToggleLandMode()
    {
        ResetModes();
        landMode = true;
    }

    public void ToggleWaterMode()
    {
        ResetModes();
        waterMode = true;
    }

    public void ToggleForestMode()
    {
        ResetModes();
        forestMode = true;
    }

    public void ToggleMountainMode()
        {
            ResetModes();
            mountainMode = true;
    }

    public void ToggleTundraMode()
    {
        ResetModes();
        tundraMode = true;
    }

    public void ToggleDesertMode()
    {
        ResetModes();
        desertMode = true;
    }

    public void ResetModes()
    {
        landMode = false;
        waterMode = false;
        forestMode = false;
        mountainMode = false;
        tundraMode = false;
        desertMode = false;
    }
}