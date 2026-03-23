using UnityEngine;
using UnityEngine.UI;

public class MapClickHandler : MonoBehaviour
{
    public RectTransform mapRect;
    public MapDataManager dataManager;
    public bool placeMode = false;
    public bool textPlaceMode = false;
    public NodeEditorUI editorUI;

    void Update()
    {
        if (!placeMode && !textPlaceMode) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMapClick(Input.mousePosition);
        }
    }

    private void HandleMapClick(Vector2 screenPosition)
    {
        if (!TryGetLocalPoint(screenPosition, out Vector2 localPoint))
            return;

        if (!TryGetNormalizedPoint(localPoint, out Vector2 normalizedPoint))
        {
            Debug.Log("Clicked outside map bounds - ignoring.");
            placeMode = false;
            textPlaceMode = false;
            return;
        }

        if (textPlaceMode)
        {
            PlaceText(normalizedPoint);
        }
        else
        {
            PlaceNode(normalizedPoint);
        }
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

    private void PlaceNode(Vector2 normalizedPoint)
    {
        dataManager.AddNode(normalizedPoint);

        editorUI.OpenEditor(dataManager.mapData.nodes.Count - 1);

        Debug.Log($"Saved node at {normalizedPoint.x}, {normalizedPoint.y}");

        placeMode = false;
    }

    private void PlaceText(Vector2 normalizedPoint)
    {
        dataManager.AddText(normalizedPoint);

        editorUI.OpenTextEditor(dataManager.mapData.mapTexts.Count - 1);

        Debug.Log($"Saved text at {normalizedPoint.x}, {normalizedPoint.y}");

        textPlaceMode = false;
    }
}