using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NodeIcon : MonoBehaviour, IPointerDownHandler
{
    int nodeIndex;
    NodeEditorUI editorUI;
    MapClickHandler mapClickHandler;

    public void Initialize(int index, NodeEditorUI ui)
    {
        nodeIndex = index;
        editorUI = ui;
        mapClickHandler = FindFirstObjectByType<MapClickHandler>();

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        editorUI.OpenEditor(nodeIndex);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            mapClickHandler.BeginNodeDrag(nodeIndex, eventData.position);
        }
    }
}
