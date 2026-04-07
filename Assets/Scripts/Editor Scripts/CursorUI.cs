using UnityEngine;
using UnityEngine.UI;

public class MapCursorManager : MonoBehaviour
{
    [Header("References")]
    public MapDrawHandler drawHandler;
    public RectTransform brushPreview; // Cursor image
    public Image previewImage; // To toggle visibility

    private RectTransform canvasRect;

    void Start()
    {
        canvasRect = brushPreview.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        previewImage = brushPreview.GetComponent<Image>();
    }

    void Update()
    {
        Vector2 localPoint;
        // Convert mouse position to a point relative to the Map
        bool isOverMap = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            drawHandler.mapRect,
            Input.mousePosition,
            null,
            out localPoint
        );

        // Check if cursor is actually inside the map bounds
        if (isOverMap && drawHandler.mapRect.rect.Contains(localPoint))
        {
            Cursor.visible = false; // Hide the standard system cursor
            previewImage.enabled = true;
            UpdateBrushVisuals();
        }
        else
        {
            Cursor.visible = true; // Show standard cursor when outside
            previewImage.enabled = false;
        }
    }

    void UpdateBrushVisuals()
    {
        // Follow the mouse
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            null,
            out mousePos
        );
        brushPreview.anchoredPosition = mousePos;

        // Calculate Size
        float currentScale = drawHandler.mapRect.localScale.x;
        float scaledSize = drawHandler.brushSize * currentScale;

        brushPreview.sizeDelta = new Vector2(scaledSize, scaledSize);
    }
}