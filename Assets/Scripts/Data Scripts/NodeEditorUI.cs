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
    [SerializeField] private NotesManager notesManager;

    private NodeData activeNode = null;
    private MapTextData activeText = null;

    //private int activeNodeIndex = -1;
    //private int activeTextIndex = -1;


    // ------------------------------ Node Editor Methods ---------------------------------

    public void OpenEditor(NodeData node)
    {
        activeNode = node;

        nodeTextInputPanel.SetActive(true);
        buttonPanel.SetActive(false);
        titleEditorPanel.SetActive(false);
        mapTextEditorPanel.SetActive(false);

        // populate fields with data from json
        //inputField.text = node.text; Text now comes from notes
        priorityDropdown.value = node.priority;
        nodeSizeSlider.value = node.size;

        notesManager.OpenNoteById(node.defaultNoteId); // Load text from note
        notesManager.LoadNotesByList(node.noteIds);

        int typeIndex = typeDropdown.options.FindIndex(
            option => option.text == node.type);

        typeDropdown.value = typeIndex >= 0 ? typeIndex : 0;

        if (!string.IsNullOrEmpty(activeNode.titleTextId))
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

    public void CreateNewNoteForActiveNode()
    {
        if (activeNode == null)
        {
            Debug.LogError("No active node selected to attach a note to!");
            return;
        }

        // Tell NotesManager to create the physical file.
        string newNoteId = notesManager.CreateNote(activeNode.id);

        // Add this ID to the node's list of associated notes.
        if (!activeNode.noteIds.Contains(newNoteId))
        {
            activeNode.noteIds.Add(newNoteId);
        }

        // Persist the change to the JSON map data.
        dataManager.Save();

        // Refresh the UI list in the Node Panel so the new button appears.
        notesManager.LoadNotesByList(activeNode.noteIds);

        Debug.Log($"Created new note {newNoteId} for node {activeNode.id}");
    }

    public void DeleteActiveNote()
    {
        // Get the ID of the note currently being edited
        string idToDelete = notesManager.GetCurrentNoteId();
        if (string.IsNullOrEmpty(idToDelete) || activeNode == null) return;

        // Deletion via NotesManager
        notesManager.DeleteNote();

        // Update Registry
        NoteRegistry.RemoveEntry(idToDelete);

        // Update Node Data List
        if (activeNode.noteIds.Contains(idToDelete))
        {
            activeNode.noteIds.Remove(idToDelete);
        }

        // Handle Default Note Logic
        if (activeNode.defaultNoteId == idToDelete)
        {
            if (activeNode.noteIds.Count > 0)
            {
                activeNode.defaultNoteId = activeNode.noteIds[0];
                // Automatically open the new default note
                notesManager.OpenNoteById(activeNode.defaultNoteId);
            }
            else
            {
                activeNode.defaultNoteId = "";
            }
        }

        // Save and Refresh
        dataManager.Save();
        notesManager.LoadNotesByList(activeNode.noteIds);
    }

    public void SetCurrentAsDefault()
    {
        string currentId = notesManager.GetCurrentNoteId();
        if (string.IsNullOrEmpty(currentId) || activeNode == null) return;

        activeNode.defaultNoteId = currentId;

        dataManager.Save();
        Debug.Log($"Note {currentId} set as default for Node {activeNode.id}");
    }

    public void SaveNodeText()
    {
        if (activeNode == null) return;

        NodeData node = activeNode;

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
        if (activeNode == null) return;

        NodeData node = activeNode;

        if (!string.IsNullOrEmpty(node.titleTextId))
        {
            MapTextData title = dataManager.mapData.mapTexts.Find(t => t.id == node.titleTextId);
            if (title != null)
                dataManager.mapData.mapTexts.Remove(title);
        }

        dataManager.mapData.nodes.Remove(activeNode);
        dataManager.Save();
        dataManager.DrawNodes();

        CloseEditor();
    }

    public void CloseEditor()
    {
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(true);
        activeNode = null;
    }


    // ------------------------------ Title Editor Methods ---------------------------------


    public void OpenTitleAppearance()
    {
        if (activeNode == null) return;

        NodeData node = activeNode;

        // if node has no title text assigned, we can't open the editor
        if (string.IsNullOrEmpty(node.titleTextId))
        {
            Debug.LogWarning("Node has no title text assigned.");
            return;
        }

        // text lookup by id
        MapTextData text = dataManager.mapData.mapTexts
            .Find(t => t.id == node.titleTextId);

        if (text == null)
        {
            Debug.LogWarning("Title text not found in mapTexts.");
            return;
        }

        OpenTitleEditor(text);
    }


    public void OpenTitleEditor(MapTextData text)
    {
        activeText = text;

        // Change Panels
        titleEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);

        // Populate fields
        titleEditorInputField.text = text.content;
        titleFontSizeSlider.value = text.fontSize;
        titleArcSlider.value = text.arc;
        titleRotationSlider.value = text.rotation;
        titleXOffsetSlider.value = text.xOffset;
        titleYOffsetSlider.value = text.yOffset;
        titlePriorityDropdown.value = text.priority;
    }


    public void saveTitleEditor()
    {
        if (activeText == null) return;

        MapTextData text = activeText;

        // save changes to text data
        text.content = titleEditorInputField.text;
        text.priority = titlePriorityDropdown.value;
        text.fontSize = titleFontSizeSlider.value;
        text.arc = titleArcSlider.value;
        text.rotation = titleRotationSlider.value;
        text.xOffset = titleXOffsetSlider.value;
        text.yOffset = titleYOffsetSlider.value;

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
        activeText = null;
    }

    public string GetText()
    {
        return inputField.text;
    }


    // ------------------------------ Text Editor Methods ---------------------------------

    public void OpenTextEditor(MapTextData text)
    {
        activeText = text;

        // Change Panels
        mapTextEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);
        titleEditorPanel.SetActive(false);

        // Populate fields
        mapTextInputField.text = text.content;
        textFontSizeSlider.value = text.fontSize;
        textArcSlider.value = text.arc;
        textRotationSlider.value = text.rotation;
        textPriorityDropdown.value = text.priority;
    }

    public void saveTextEditor()
    {
        if (activeText == null) return;

        MapTextData text = activeText;

        // save changes to text data
        text.content = mapTextInputField.text;
        text.priority = textPriorityDropdown.value;
        text.fontSize = textFontSizeSlider.value;
        text.arc = textArcSlider.value;
        text.rotation = textRotationSlider.value;


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
        activeText = null;
    }

    public void DeleteActiveText()
    {
        if (activeText == null) return;

        MapTextData textToDelete = activeText;

        // Check if this text is a title of any node
        NodeData attachedNode = dataManager.mapData.nodes
            .Find(n => n.titleTextId == activeText.id);

        if (attachedNode != null)
        {
            attachedNode.titleTextId = "";
        }

        // Remove the text
        dataManager.mapData.mapTexts.Remove(activeText);

        // Save and redraw
        dataManager.Save();
        dataManager.DrawMapTexts();
        dataManager.DrawNodes();

        // Close whichever editor is open
        CloseTextEditor();
        CloseTitleEditor();

        activeText = null;
    }

}