using UnityEngine;
using UnityEngine.UI;

public class NodeIcon : MonoBehaviour
{
    int nodeIndex;
    NodeEditorUI editorUI;

    public void Initialize(int index, NodeEditorUI ui)
    {
        nodeIndex = index;
        editorUI = ui;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        editorUI.OpenEditor(nodeIndex);
    }
}
