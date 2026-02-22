using UnityEngine;
using UnityEngine.UI;

public class MapClickHandler : MonoBehaviour
{
    public RectTransform mapRect;
    public MapDataManager dataManager;
    public bool placeMode = false;

    void Update()
    {
        if (!placeMode) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapRect,
                Input.mousePosition,
                null,
                out localPoint))
            {
                Rect rect = mapRect.rect;

                // Convert to 0–1 range
                float normalizedX = (localPoint.x - rect.x) / rect.width;
                float normalizedY = (localPoint.y - rect.y) / rect.height;

                // Ignore clicks outside map bounds
                if (normalizedX < 0f || normalizedX > 1f ||
                    normalizedY < 0f || normalizedY > 1f)
                {
                    Debug.Log("Clicked outside map bounds - ignoring.");
                    placeMode = false;   // Optional: exit place mode
                    return;
                }

                dataManager.AddNode(new Vector2(normalizedX, normalizedY));

                Debug.Log($"Saved node at {normalizedX}, {normalizedY}");

                placeMode = false;
            }
        }
    }
}