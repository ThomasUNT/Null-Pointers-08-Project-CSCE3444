using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;

public class NodeEditorUI : MonoBehaviour
{
    // Panels
    [Header("Panels")]
    [SerializeField] private GameObject nodeTextInputPanel;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject titleEditorPanel;
    [SerializeField] private GameObject mapTextEditorPanel;

    // Node Editor Fields
    [Header("Node Editor Fields")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_InputField titleInputField;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown priorityDropdown;
    [SerializeField] private UnityEngine.UI.Slider nodeSizeSlider;

    // Title Editor Fields
    [Header("Title Editor Fields")]
    [SerializeField] private TMP_InputField titleEditorInputField;
    [SerializeField] private UnityEngine.UI.Slider titleFontSizeSlider;
    [SerializeField] private UnityEngine.UI.Slider titleArcSlider;
    [SerializeField] private UnityEngine.UI.Slider titleRotationSlider;
    [SerializeField] private UnityEngine.UI.Slider titleXOffsetSlider;
    [SerializeField] private UnityEngine.UI.Slider titleYOffsetSlider;
    [SerializeField] private TMP_Dropdown titlePriorityDropdown;

    // Text Editor Fields
    [Header("Text Editor Panels")]
    [SerializeField] private TMP_InputField mapTextInputField;
    [SerializeField] private UnityEngine.UI.Slider textFontSizeSlider;
    [SerializeField] private UnityEngine.UI.Slider textArcSlider;
    [SerializeField] private UnityEngine.UI.Slider textRotationSlider;
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
        titleEditorPanel.SetActive(false);
        mapTextEditorPanel.SetActive(false);

        // populate fields with data from json
        NodeData node = dataManager.mapData.nodes[nodeIndex];

        inputField.text = node.text;
        priorityDropdown.value = node.priority;
        nodeSizeSlider.value = node.size;

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
        node.size = nodeSizeSlider.value;

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
                    existing.y = node.y;
                }
            }
            else
            {
                // create new title text entry
                MapTextData newText = new MapTextData();
                newText.id = System.Guid.NewGuid().ToString();
                newText.content = newTitleText;
                newText.x = node.x;
                newText.y = node.y;
                newText.yOffset = -normalizedYOffset; //Store default offset
                newText.xOffset = 0f;
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


    // ------------------------------ Title Editor Methods ---------------------------------


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

        OpenTitleEditor(textIndex);
    }


    public void OpenTitleEditor(int textIndex)
    {
        activeTextIndex = textIndex;

        // Change Panels
        titleEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);

        MapTextData textData = dataManager.mapData.mapTexts[textIndex];

        // Populate fields
        titleEditorInputField.text = textData.content;
        titleFontSizeSlider.value = textData.fontSize;
        titleArcSlider.value = textData.arc;
        titleRotationSlider.value = textData.rotation;
        titleXOffsetSlider.value = textData.xOffset;
        titleYOffsetSlider.value = textData.yOffset;
        titlePriorityDropdown.value = textData.priority;
    }


    public void saveTitleEditor()
    {
        if (activeTextIndex < 0) return;

        MapTextData textData = dataManager.mapData.mapTexts[activeTextIndex];

        // save changes to text data
        textData.content = titleEditorInputField.text;
        textData.priority = titlePriorityDropdown.value;
        textData.fontSize = titleFontSizeSlider.value;
        textData.arc = titleArcSlider.value;
        textData.rotation = titleRotationSlider.value;
        textData.xOffset = titleXOffsetSlider.value;
        textData.yOffset = titleYOffsetSlider.value;

        // Save data
        dataManager.Save();

        // Redraw map texts to reflect changes
        dataManager.DrawMapTexts();

        CloseTitleEditor();
    }


    public void CloseTitleEditor()
    {
        titleEditorPanel.SetActive(false);
        buttonPanel.SetActive(true);
        activeTextIndex = -1;
    }

    public string GetText()
    {
        return inputField.text;
    }


    // ------------------------------ Text Editor Methods ---------------------------------

    public void OpenTextEditor(int textIndex)
    {
        activeTextIndex = textIndex;

        // Change Panels
        mapTextEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);
        titleEditorPanel.SetActive(false);

        MapTextData textData = dataManager.mapData.mapTexts[textIndex];

        // Populate fields
        mapTextInputField.text = textData.content;
        textFontSizeSlider.value = textData.fontSize;
        textArcSlider.value = textData.arc;
        textRotationSlider.value = textData.rotation;
        textPriorityDropdown.value = textData.priority;
    }

    public void saveTextEditor()
    {
        if (activeTextIndex < 0) return;

        MapTextData textData = dataManager.mapData.mapTexts[activeTextIndex];

        // save changes to text data
        textData.content = mapTextInputField.text;
        textData.priority = textPriorityDropdown.value;
        textData.fontSize = textFontSizeSlider.value;
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
        mapTextEditorPanel.SetActive(false);
        buttonPanel.SetActive(true);
        activeTextIndex = -1;
    }

}