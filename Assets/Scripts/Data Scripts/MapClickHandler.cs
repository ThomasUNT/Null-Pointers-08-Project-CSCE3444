using UnityEngine;
using UnityEngine.UI;

public class MapClickHandler : MonoBehaviour
{
    public RectTransform mapRect;
    public MapDataManager dataManager;
    public bool placeMode = false;
    public bool textPlaceMode = false;
    public NodeEditorUI editorUI;

    // Move Stuff
    public bool isDragging = false;

    private NodeData draggingNode;
    private MapTextData draggingText;

    private Vector2 dragStartMousePos;
    private Vector2 dragStartObjectPos;


    void Update()
    {
        HandleDragging();

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
        NodeData newNode = dataManager.AddNode(normalizedPoint);

        editorUI.OpenEditor(newNode);

        Debug.Log($"Saved node at {normalizedPoint.x}, {normalizedPoint.y}");

        placeMode = false;
    }

    private void PlaceText(Vector2 normalizedPoint)
    {
        MapTextData newText = dataManager.AddText(normalizedPoint);

        editorUI.OpenTextEditor(newText);

        Debug.Log($"Saved text at {normalizedPoint.x}, {normalizedPoint.y}");

        textPlaceMode = false;
    }


    // ------------- Drag Nodes and Texts -------------------


    public void BeginNodeDrag(NodeData node, Vector2 screenPosition)
    {
        if (!TryGetLocalPoint(screenPosition, out Vector2 localPoint))
            return;

        isDragging = true;
        draggingNode = node;
        draggingText = null;

        dragStartMousePos = localPoint;
        dragStartObjectPos = new Vector2(node.x, node.y);
    }

    public void BeginTextDrag(MapTextData mapText, Vector2 screenPosition)
    {
        if (mapText == null)
        {
            Debug.LogError("BeginTextDrag received NULL mapText!");
            return;
        }

        if (!TryGetLocalPoint(screenPosition, out Vector2 localPoint))
            return;

        isDragging = true;
        draggingText = mapText;
        draggingNode = null;

        dragStartMousePos = localPoint;
        dragStartObjectPos = new Vector2(mapText.x, mapText.y);
    }

    private void HandleDragging()
    {
        if (!isDragging) return;

        // Convert mouse to local space
        if (!TryGetLocalPoint(Input.mousePosition, out Vector2 localPoint))
            return;

        // Calculate delta movement
        Vector2 delta = (localPoint - dragStartMousePos) / mapRect.rect.size;

        // Move node and attached title text if exists
        if (draggingNode != null)
        {
            NodeData node = draggingNode;

            float newX = Mathf.Clamp(dragStartObjectPos.x + delta.x, 0f, 1f);
            float newY = Mathf.Clamp(dragStartObjectPos.y + delta.y, 0f, 1f);

            // Calculate how much the node actually moved
            float deltaX = newX - node.x;
            float deltaY = newY - node.y;

            node.x = newX;
            node.y = newY;

            // Move attached title text if it exists
            if (!string.IsNullOrEmpty(node.titleTextId))
            {
                MapTextData titleText = dataManager.mapData.mapTexts
                    .Find(t => t.id == node.titleTextId);

                if (titleText != null)
                {
                    titleText.x += deltaX;
                    titleText.y += deltaY;
                }
            }

            dataManager.DrawNodes();
            dataManager.DrawMapTexts();
        }

        // Move text (and node if this text is a title)
        if (draggingText != null)
        {
            MapTextData text = draggingText;

            float newX = Mathf.Clamp(dragStartObjectPos.x + delta.x, 0f, 1f);
            float newY = Mathf.Clamp(dragStartObjectPos.y + delta.y, 0f, 1f);

            // Calculate actual movement delta
            float deltaX = newX - text.x;
            float deltaY = newY - text.y;

            text.x = newX;
            text.y = newY;

            // Check if this text is a node title
            NodeData node = dataManager.mapData.nodes
                .Find(n => n.titleTextId == text.id);

            if (node != null)
            {
                node.x += deltaX;
                node.y += deltaY;
            }

            dataManager.DrawNodes();
            dataManager.DrawMapTexts();
        }

        // Stop dragging when mouse released
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            draggingNode = null;
            draggingText = null;

            dataManager.Save();
        }
    }
}