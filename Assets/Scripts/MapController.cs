using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    public RectTransform mapWindow; // MapWindow panel
    public float zoomSpeed = 0.1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    private RectTransform mapRT;

    private Vector2 lastMousePos;

    void Awake()
    {
        mapRT = GetComponent<RectTransform>();
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
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

        ClampToWindow();
    }


    void HandlePan()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePos = Input.mousePosition; // On click, store the initial mouse position
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
                mapRT.anchoredPosition += delta;
                lastMousePos = Input.mousePosition; // While dragging, move map and update last mouse position

                ClampToWindow();
            }
        }
    }

    void ClampToWindow()
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
