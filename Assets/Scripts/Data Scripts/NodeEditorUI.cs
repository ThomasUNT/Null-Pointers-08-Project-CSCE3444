using UnityEngine;
using TMPro;

public class NodeEditorUI : MonoBehaviour
{
    public GameObject nodeTextInputPanel;
    public GameObject buttonPanel;
    public TMP_InputField inputField;
    public MapDataManager dataManager;

    private int activeNodeIndex = -1;

    public void OpenEditor(int nodeIndex)
    {
        activeNodeIndex = nodeIndex;

        Debug.Log("OpenEditor called for node " + nodeIndex);

        nodeTextInputPanel.SetActive(true);
        buttonPanel.SetActive(false);

        inputField.text = dataManager.mapData.nodes[nodeIndex].text;

        // focus cursor automatically
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void SaveNodeText()
    {
        if (activeNodeIndex < 0) return;

        dataManager.mapData.nodes[activeNodeIndex].text = inputField.text;

        dataManager.Save();

        CloseEditor();
    }


    public void DeleteNode()
    {
        if (activeNodeIndex < 0) return;

        dataManager.mapData.nodes.RemoveAt(activeNodeIndex);
        dataManager.Save();
        dataManager.DrawNodes();

        CloseEditor();
    }


    public void CloseEditor()
    {
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(true);
        activeNodeIndex = -1;
    }

    public string GetText()
    {
        return inputField.text;
    }
}
