using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapTextIcon : MonoBehaviour, IPointerDownHandler
{
    private MapTextData mapTextData;
    int textIndex;
    string textId;

    NodeEditorUI editorUI;
    MapClickHandler mapClickHandler;
    MapData mapData;

    public void Initialize(MapTextData mapText, NodeEditorUI ui, MapData data)
    {
        editorUI = ui;
        mapData = data;
        mapTextData = mapText;

        mapClickHandler = FindFirstObjectByType<MapClickHandler>();

        textId = mapText.id;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // Check if this text is tied to a node title
        NodeData node = mapData.nodes.Find(n => n.titleTextId == textId);

        if (node != null)
        {
            editorUI.CloseTextEditor();
            editorUI.OpenTitleEditor(mapTextData);
        }
        else
        {
            editorUI.CloseTitleEditor();
            editorUI.OpenTextEditor(mapTextData);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            mapClickHandler.BeginTextDrag(mapTextData, eventData.position);
        }
    }
}