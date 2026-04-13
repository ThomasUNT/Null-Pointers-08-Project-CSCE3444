using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    [Header("References")]
    public MapDrawHandler drawHandler;
    public RectTransform brushPreview; // Cursor image
    public RectTransform mapWindow;
    public Image previewImage; // To toggle visibility

    private RectTransform canvasRect;

    void Start()
    {
        canvasRect = brushPreview.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        previewImage = brushPreview.GetComponent<Image>();
    }

    void Update()
    {
        // Ask the DrawHandler if the mouse is physically over the map and not blocked by UI
        bool canDrawHere = drawHandler.IsMouseDirectlyOverMap();

        // Check if we are in a draw mode AND the map is the top-most object
        if (canDrawHere && CheckDrawModes())
        {
            Cursor.visible = false;
            previewImage.enabled = true;
            UpdateBrushVisuals();
        }
        else
        {
            Cursor.visible = true;
            previewImage.enabled = false;
        }
    }

    bool CheckDrawModes()
    {
        if (drawHandler.landMode || drawHandler.waterMode || drawHandler.forestMode ||
            drawHandler.mountainMode || drawHandler.tundraMode || drawHandler.desertMode)
        {
            return true;
        }
        else
        {
            return false;
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
        float scaledSize = drawHandler.brushSize * currentScale * 1.6f;

        brushPreview.sizeDelta = new Vector2(scaledSize, scaledSize);
    }
}