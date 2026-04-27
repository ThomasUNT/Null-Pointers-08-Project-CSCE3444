using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MapController : MonoBehaviour
{
    public RectTransform mapWindow; // MapWindow panel
    public float zoomSpeed = 0.1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;
    public RectTransform mapRect;
    public MapDataManager mapDataManager;

    private RectTransform mapRT;
    private Vector2 lastMousePos;
    private bool isPanning = false;

    void Awake()
    {
        mapRT = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(
        mapRect,
        Input.mousePosition,
        null))
        {
            return; // mouse not over map
        }

        if (!isPanning && IsMouseOverBlockingUI())
        {
            return;
        }

        HandleZoom();
        HandlePan();
    }

    //Checks if there is UI between the mouse and the map
    private bool IsMouseOverBlockingUI()
    {
        if (EventSystem.current == null) return false;

        // Create a PointerEventData with the current mouse position
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();

        // Raycast against all UI elements
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            // If the topmost object is NOT the mapRect or a child of it, then we're over blocking UI
            return !results[0].gameObject.transform.IsChildOf(mapRect);
        }
        return false;
    }

    void HandleZoom()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll == 0) return;

        // Current scale
        float oldScale = mapRT.localScale.x;

        // Compute new scale
        float newScale = Mathf.Clamp(oldScale + scroll * zoomSpeed, minScale, maxScale);

        // Get mouse position in MapWindow local space
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapWindow,
            Input.mousePosition,
            null,
            out localMousePos
        );

        // Compute normalized position (0 = map pivot, 1 = mouse)
        Vector2 pivotDelta = localMousePos - mapRT.anchoredPosition;

        // Scale the map
        mapRT.localScale = new Vector3(newScale, newScale, 1f);

        // Adjust anchoredPosition so point under mouse stays put
        Vector2 newPivotDelta = pivotDelta * (newScale / oldScale);
        mapRT.anchoredPosition += pivotDelta - newPivotDelta;

        if (mapDataManager != null)
        {
            mapDataManager.DrawNodes();
            mapDataManager.DrawMapTexts(); 
        }

        ClampToWindow();
    }


    void HandlePan()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetMouseButtonDown(0))
            {
                isPanning = true;
                lastMousePos = Input.mousePosition; // On click, store the initial mouse position
            }

            if (Input.GetMouseButton(0) && isPanning)
            {
                Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
                mapRT.anchoredPosition += delta;
                lastMousePos = Input.mousePosition; // While dragging, move map and update last mouse position

                ClampToWindow();
            }
        }
    }

    public void ClampToWindow()
    {
        Vector2 mapSize = mapRT.sizeDelta * mapRT.localScale * 0.5f;
        Vector2 windowSize = mapWindow.rect.size * 0.5f;

        float xMax = mapSize.x > windowSize.x ? mapSize.x - windowSize.x : 0;
        float xMin = -xMax;

        float yMax = mapSize.y > windowSize.y ? mapSize.y - windowSize.y : 0;
        float yMin = -yMax;

        Vector2 pos = mapRT.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, xMin, xMax);
        pos.y = Mathf.Clamp(pos.y, yMin, yMax);

        mapRT.anchoredPosition = pos;
    }
}
