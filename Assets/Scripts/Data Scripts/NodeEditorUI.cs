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
        NodeData node = dataManager.mapData.nodes[nodeIndex];

        inputField.text = node.text;
        priorityDropdown.value = node.priority; 

        int typeIndex = typeDropdown.options.FindIndex(
            option => option.text == node.type);

        typeDropdown.value = typeIndex >= 0 ? typeIndex : 0;

        if (!string.IsNullOrEmpty(node.titleTextId))
        {
            MapTextData titleData = dataManager.mapData.mapTexts.Find(t => t.id == node.titleTextId);

            if (titleData != null)
            {
                titleInputField.text = titleData.content;
            }
        }
        else
        {
            titleInputField.text = "";
        }

            // focus cursor automatically
            inputField.ActivateInputField();
        inputField.Select();
    }

    public void SaveNodeText()
    {
        if (activeNodeIndex < 0) return;

        NodeData node = dataManager.mapData.nodes[activeNodeIndex];

        node.text = inputField.text;
        node.type = typeDropdown.options[typeDropdown.value].text;
        node.priority = priorityDropdown.value;

        string newTitleText = titleInputField.text;

        // if title text is empty, remove existing title text entry if it exists
        if (string.IsNullOrEmpty(newTitleText))
        {
            if(!string.IsNullOrEmpty(node.titleTextId))
            {
                MapTextData existing = dataManager.mapData.mapTexts.Find(t => t.id == node.titleTextId);

                if (existing != null) dataManager.mapData.mapTexts.Remove(existing);

                node.titleTextId = "";
            }
        }
        else
        {
            // normalize offset
            Rect rect = dataManager.mapRect.rect;

            float normalizedYOffset = 20f / rect.height;

            // If node already has a title text entry, update it. Otherwise create a new one.
            if (!string.IsNullOrEmpty(node.titleTextId))
            {
                MapTextData existing = dataManager.mapData.mapTexts.Find(t => t.id == node.titleTextId);
                if (existing != null)
                {
                    existing.content = newTitleText;
                    existing.x = node.x;
                    existing.y = node.y - normalizedYOffset; // offset text below node icon
                }
            }
            else
            {
                // create new title text entry
                MapTextData newText = new MapTextData();
                newText.id = System.Guid.NewGuid().ToString();
                newText.content = newTitleText;
                newText.x = node.x;
                newText.y = node.y - normalizedYOffset; // offset text below node icon
                newText.fontSize = 14;
                newText.priority = 0;
                newText.colorHex = "#FFFFFF";
                newText.rotation = 0f;
                newText.arc = 0f;

                dataManager.mapData.mapTexts.Add(newText);

                node.titleTextId = newText.id;
            }
        }

        dataManager.Save();
        dataManager.DrawMapTexts();

        CloseEditor();
    }


    public void DeleteNode()
    {
        if (activeNodeIndex < 0) return;

        NodeData node = dataManager.mapData.nodes[activeNodeIndex];

        if (!string.IsNullOrEmpty(node.titleTextId))
        {
            MapTextData title = dataManager.mapData.mapTexts.Find(t => t.id == node.titleTextId);
            if (title != null)
                dataManager.mapData.mapTexts.Remove(title);
        }

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
