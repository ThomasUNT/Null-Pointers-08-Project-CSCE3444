using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NodeEditorUI : MonoBehaviour
{
    // Panels
    [SerializeField] private GameObject nodeTextInputPanel;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject textEditorPanel;

    // Node Editor Fields
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown priorityDropdown;

    // Text Editor Fields
    [SerializeField] private TMP_InputField mapTextInputField;
    [SerializeField] private Slider fontSizeSlider;
    [SerializeField] private Slider textArcSlider;
    [SerializeField] private Slider textRotationSlider;
    [SerializeField] private TMP_Dropdown textPriorityDropdown;

    [SerializeField] private MapDataManager dataManager;

    private int activeNodeIndex = -1;
    private int activeTextIndex = -1;


    // ------------------------------ Node Editor Methods ---------------------------------

    public void OpenEditor(int nodeIndex)
    {
        activeNodeIndex = nodeIndex;

        Debug.Log("OpenEditor called for node " + nodeIndex);

        nodeTextInputPanel.SetActive(true);
        buttonPanel.SetActive(false);
        textEditorPanel.SetActive(false);

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
            if (!string.IsNullOrEmpty(node.titleTextId))
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
        dataManager.DrawNodes();
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


    // ------------------------------ Text Editor Methods ---------------------------------


    public void OpenTitleAppearance()
    {
        if (activeNodeIndex < 0) return;

        NodeData node = dataManager.mapData.nodes[activeNodeIndex];

        // if node has no title text assigned, we can't open the editor
        if (string.IsNullOrEmpty(node.titleTextId))
        {
            Debug.LogWarning("Node has no title text assigned.");
            return;
        }

        // text lookup by id
        int textIndex = dataManager.mapData.mapTexts
            .FindIndex(t => t.id == node.titleTextId);

        if (textIndex < 0)
        {
            Debug.LogWarning("Title text not found in mapTexts.");
            return;
        }

        OpenTextEditor(textIndex);
    }


    public void OpenTextEditor(int textIndex)
    {
        activeTextIndex = textIndex;

        // Change Panels
        textEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);

        MapTextData textData = dataManager.mapData.mapTexts[textIndex];

        // Populate fields
        mapTextInputField.text = textData.content;
        fontSizeSlider.value = textData.fontSize;
        textArcSlider.value = textData.arc;
        textRotationSlider.value = textData.rotation;
        textPriorityDropdown.value = textData.priority;
    }


    public void SaveTextEditor()
    {
        if (activeTextIndex < 0) return;

        MapTextData textData = dataManager.mapData.mapTexts[activeTextIndex];

        // save changes to text data
        textData.content = mapTextInputField.text;
        textData.priority = textPriorityDropdown.value;
        textData.fontSize = fontSizeSlider.value;
        textData.arc = textArcSlider.value;
        textData.rotation = textRotationSlider.value;

        // Save data
        dataManager.Save();

        // Redraw map texts to reflect changes
        dataManager.DrawMapTexts();

        CloseTextEditor();
    }


    public void CloseTextEditor()
    {
        textEditorPanel.SetActive(false);
        buttonPanel.SetActive(true);
        activeTextIndex = -1;
    }

    public string GetText()
    {
        return inputField.text;
    }
}
