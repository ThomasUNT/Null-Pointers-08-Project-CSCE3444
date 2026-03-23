using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MapTextIcon : MonoBehaviour, IPointerDownHandler
{
    int textIndex;
    string textId;

    NodeEditorUI editorUI;
    MapClickHandler mapClickHandler;
    MapData mapData;

    public void Initialize(int index, NodeEditorUI ui, MapData data)
    {
        textIndex = index;
        editorUI = ui;
        mapData = data;

        mapClickHandler = FindFirstObjectByType<MapClickHandler>();

        textId = mapData.mapTexts[index].id;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // Check if this text is tied to a node title
        NodeData node = mapData.nodes.Find(n => n.titleTextId == textId);

        if (node != null)
        {
            editorUI.CloseTextEditor();
            editorUI.OpenTitleEditor(
                mapData.mapTexts.FindIndex(t => t.id == textId)
            );
        }
        else
        {
            editorUI.CloseTitleEditor();
            editorUI.OpenTextEditor(textIndex);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            mapClickHandler.BeginTextDrag(textIndex, eventData.position);
        }
    }
}