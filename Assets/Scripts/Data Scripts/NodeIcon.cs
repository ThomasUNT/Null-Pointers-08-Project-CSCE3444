using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodeIcon : MonoBehaviour, IPointerDownHandler
{
    private NodeData nodeData;
    int nodeIndex;
    NodeEditorUI editorUI;
    MapClickHandler mapClickHandler;

    public void Initialize(NodeData node, NodeEditorUI ui)
    {
        nodeData = node;
        editorUI = ui;
        mapClickHandler = FindFirstObjectByType<MapClickHandler>();

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        editorUI.OpenEditor(nodeData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            mapClickHandler.BeginNodeDrag(nodeData, eventData.position);
        }
    }
}
