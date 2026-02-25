using UnityEngine;
using TMPro;

public class NodeEditorUI : MonoBehaviour
{
    public GameObject nodeTextInputPanel;
    public GameObject buttonPanel;
    public TMP_InputField inputField;
    public TMP_InputField titleInputField;
    public TMP_Dropdown typeDropdown;
    public TMP_Dropdown priorityDropdown;
    public MapDataManager dataManager;

    private int activeNodeIndex = -1;

    public void OpenEditor(int nodeIndex)
    {
        activeNodeIndex = nodeIndex;

        Debug.Log("OpenEditor called for node " + nodeIndex);

        nodeTextInputPanel.SetActive(true);
        buttonPanel.SetActive(false);

        // populate fields with data from json
        inputField.text = dataManager.mapData.nodes[nodeIndex].text;
        titleInputField.text = dataManager.mapData.nodes[nodeIndex].title;
        priorityDropdown.value = dataManager.mapData.nodes[nodeIndex].priority; 

        int typeIndex = typeDropdown.options.FindIndex(
            option => option.text == dataManager.mapData.nodes[nodeIndex].type);

        typeDropdown.value = typeIndex >= 0 ? typeIndex : 0;

        // focus cursor automatically
        inputField.ActivateInputField();
        inputField.Select();
    }

    public void SaveNodeText()
    {
        if (activeNodeIndex < 0) return;

        dataManager.mapData.nodes[activeNodeIndex].text = inputField.text;
        dataManager.mapData.nodes[activeNodeIndex].title = titleInputField.text;
        dataManager.mapData.nodes[activeNodeIndex].type = typeDropdown.options[typeDropdown.value].text;
        dataManager.mapData.nodes[activeNodeIndex].priority = priorityDropdown.value;

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
