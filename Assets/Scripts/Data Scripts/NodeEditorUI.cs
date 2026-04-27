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
    [SerializeField] private ColorPickerPanel colorPicker;

    private NodeData activeNode = null;
    private MapTextData activeText = null;

    private bool isInitializing = false; // Flag to prevent live update triggers during setup
    public bool viewOnlyMode = false; // If true, all editing features disabled


    void Start()
    {
        if (!viewOnlyMode)
        {
            // Node Listeners
            titleInputField.onValueChanged.AddListener(delegate { LiveUpdateNodeTitle(); });
            nodeSizeSlider.onValueChanged.AddListener(delegate { LiveUpdateNode(); });
            typeDropdown.onValueChanged.AddListener(delegate { LiveUpdateNode(); });
            priorityDropdown.onValueChanged.AddListener(delegate { LiveUpdateNode(); });

            // Title Editor Listeners
            titleEditorInputField.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });
            titleFontSizeSlider.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });
            titleArcSlider.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });
            titleRotationSlider.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });
            titleXOffsetSlider.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });
            titleYOffsetSlider.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });
            titlePriorityDropdown.onValueChanged.AddListener(delegate { LiveUpdateTitle(); });

            // Map Text Listeners
            mapTextInputField.onValueChanged.AddListener(delegate { LiveUpdateText(); });
            textFontSizeSlider.onValueChanged.AddListener(delegate { LiveUpdateText(); });
            textArcSlider.onValueChanged.AddListener(delegate { LiveUpdateText(); });
            textRotationSlider.onValueChanged.AddListener(delegate { LiveUpdateText(); });
            textPriorityDropdown.onValueChanged.AddListener(delegate { LiveUpdateText(); });
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapePress();
        }
    }

    // ------------------------------ Live Update Helpers ---------------------------------

    private void LiveUpdateNode()
    {
        if (activeNode == null || isInitializing) return;
        activeNode.size = nodeSizeSlider.value;
        activeNode.type = typeDropdown.options[typeDropdown.value].text;
        activeNode.priority = priorityDropdown.value;
        dataManager.DrawNodes(); // Redraw immediately
    }

    private void LiveUpdateNodeTitle()
    {
        if (activeNode == null || isInitializing) return;

        string newTitleText = titleInputField.text;

        // CASE 1: Box is empty -> Remove the title if it exists
        if (string.IsNullOrEmpty(newTitleText))
        {
            if (!string.IsNullOrEmpty(activeNode.titleTextId))
            {
                MapTextData existing = dataManager.mapData.mapTexts.Find(t => t.id == activeNode.titleTextId);
                if (existing != null) dataManager.mapData.mapTexts.Remove(existing);

                activeNode.titleTextId = "";
            }
        }
        // CASE 2: Box has text -> Update or Create
        else
        {
            if (!string.IsNullOrEmpty(activeNode.titleTextId))
            {
                // Update existing
                MapTextData existing = dataManager.mapData.mapTexts.Find(t => t.id == activeNode.titleTextId);
                if (existing != null)
                {
                    existing.content = newTitleText;
                    // Keep it synced to node position
                    existing.x = activeNode.x;
                    existing.y = activeNode.y;
                }
            }
            else
            {
                // Create new title
                MapTextData newText = CreateDefaultTitleObject(newTitleText);
                dataManager.mapData.mapTexts.Add(newText);
                activeNode.titleTextId = newText.id;
            }
        }

        // Redraw
        dataManager.DrawMapTexts();
    }

    private void LiveUpdateTitle()
    {
        if (activeText == null || isInitializing) return;

        activeText.content = titleEditorInputField.text;
        activeText.fontSize = titleFontSizeSlider.value;
        activeText.arc = titleArcSlider.value;
        activeText.rotation = titleRotationSlider.value;
        activeText.xOffset = titleXOffsetSlider.value;
        activeText.yOffset = titleYOffsetSlider.value;
        activeText.priority = titlePriorityDropdown.value;

        dataManager.DrawMapTexts(); // Redraw immediately
    }

    private void LiveUpdateText()
    {
        if (activeText == null || isInitializing) return;

        activeText.content = mapTextInputField.text;
        activeText.fontSize = textFontSizeSlider.value;
        activeText.arc = textArcSlider.value;
        activeText.rotation = textRotationSlider.value;
        activeText.priority = textPriorityDropdown.value;

        dataManager.DrawMapTexts(); // Redraw immediately
    }

    // ------------------------------ Node Editor Methods ---------------------------------

    public void OpenEditor(NodeData node)
    {
        if (activeNode != null || activeText != null)
        {
            dataManager.Load();
            dataManager.DrawNodes();
            dataManager.DrawMapTexts();
        }

        activeNode = dataManager.mapData.nodes.Find(n => n.id == node.id);
        if (activeNode == null) return;

        isInitializing = true;

        nodeTextInputPanel.SetActive(true);
        buttonPanel.SetActive(false);

        if (!viewOnlyMode)
        {
            titleEditorPanel.SetActive(false);
            mapTextEditorPanel.SetActive(false);

            // populate fields with data from json
            priorityDropdown.value = activeNode.priority;
            nodeSizeSlider.value = activeNode.size;

            int typeIndex = typeDropdown.options.FindIndex(
            option => option.text == activeNode.type);

            typeDropdown.value = typeIndex >= 0 ? typeIndex : 0;
        }

        if (!string.IsNullOrEmpty(activeNode.defaultNoteId))
        {
            notesManager.OpenNoteById(activeNode.defaultNoteId);
        }
        else
        {
            notesManager.ClearEditorUI();
        }

        notesManager.LoadNotesByList(activeNode.noteIds);

        if (!string.IsNullOrEmpty(activeNode.titleTextId))
        {
            MapTextData titleData = dataManager.mapData.mapTexts.Find(t => t.id == activeNode.titleTextId);

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

        isInitializing = false;
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

    private MapTextData CreateDefaultTitleObject(string content)
    {
        Rect rect = dataManager.mapRect.rect;
        float normalizedYOffset = 20f / rect.height;

        return new MapTextData()
        {
            id = System.Guid.NewGuid().ToString(),
            content = content,
            x = activeNode.x,
            y = activeNode.y,
            yOffset = -normalizedYOffset,
            xOffset = 0f,
            fontSize = 14,
            priority = 0,
            colorHex = "#FFFFFF",
            rotation = 0f,
            arc = 0f
        };
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

        // Final sync for non-title node data
        activeNode.text = inputField.text;
        activeNode.type = typeDropdown.options[typeDropdown.value].text;
        activeNode.priority = priorityDropdown.value;
        activeNode.size = nodeSizeSlider.value;

        dataManager.Save(); // Writes the current state (including new titles) to JSON
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

    public void CancelNodeEditor()
    {
        // We discard changes by reloading the original JSON from disk
        dataManager.Load();
        dataManager.DrawNodes();
        dataManager.DrawMapTexts();
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

        string titleId = activeNode.titleTextId;

        SaveNodeText();

        // if node has no title text assigned, we can't open the editor
        if (string.IsNullOrEmpty(titleId))
        {
            Debug.LogWarning("Node has no title text assigned.");
            return;
        }

        // text lookup by id
        MapTextData text = dataManager.mapData.mapTexts
            .Find(t => t.id == titleId);

        if (text == null)
        {
            Debug.LogWarning("Title text not found in mapTexts.");
            return;
        }

        OpenTitleEditor(text);
    }


    public void OpenTitleEditor(MapTextData text)
    {
        if (viewOnlyMode)
        {
            // If view only, just open the node instead
            OpenEditor(dataManager.mapData.nodes.Find(n => n.titleTextId == text.id));
            return;
        }

        if (activeNode != null || activeText != null)
        {
            dataManager.Load();
            dataManager.DrawNodes();
            dataManager.DrawMapTexts();
        }

        activeText = dataManager.mapData.mapTexts.Find(t => t.id == text.id);
        if (activeText == null) return;

        isInitializing = true;

        // Change Panels
        titleEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);

        // Populate fields
        titleEditorInputField.text = activeText.content;
        titleFontSizeSlider.value = activeText.fontSize;
        titleArcSlider.value = activeText.arc;
        titleRotationSlider.value = activeText.rotation;
        titleXOffsetSlider.value = activeText.xOffset;
        titleYOffsetSlider.value = activeText.yOffset;
        titlePriorityDropdown.value = activeText.priority;

        isInitializing = false;
    }

    public void OnPickMapTextColorClicked()
    {
        if (activeText == null) return;

        if (colorPicker.gameObject.activeSelf)
        {
            colorPicker.Close();
            return;
        }

        // Pass the current color and a set of instructions on what to do when it changes
        colorPicker.Initialize(activeText.GetColor(), (newColor) => {
            activeText.colorHex = "#" + ColorUtility.ToHtmlStringRGB(newColor);
            dataManager.DrawMapTexts(); // Live preview
        });
    }

    public void saveTitleEditor()
    {
        if (activeText == null) return;
        dataManager.Save(); // Save the live changes to disk
        CloseTitleEditor();
    }

    public void CancelTitleEditor()
    {
        dataManager.Load(); // Revert to version from json
        dataManager.DrawMapTexts();
        CloseTitleEditor();
    }

    public void CloseTitleEditor()
    {
        titleEditorPanel.SetActive(false);
        colorPicker.Close();
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
        if (activeNode != null || activeText != null)
        {
            dataManager.Load();
            dataManager.DrawNodes();
            dataManager.DrawMapTexts();
        }

        activeText = dataManager.mapData.mapTexts.Find(t => t.id == text.id);
        if (activeText == null) return;

        isInitializing = true;

        // Change Panels
        mapTextEditorPanel.SetActive(true);
        nodeTextInputPanel.SetActive(false);
        buttonPanel.SetActive(false);
        titleEditorPanel.SetActive(false);

        // Populate fields
        mapTextInputField.text = activeText.content;
        textFontSizeSlider.value = activeText.fontSize;
        textArcSlider.value = activeText.arc;
        textRotationSlider.value = activeText.rotation;
        textPriorityDropdown.value = activeText.priority;

        isInitializing = false;
    }

    public void saveTextEditor()
    {
        if (activeText == null) return;
        dataManager.Save(); // Save the live changes to disk
        CloseTextEditor();
    }

    public void CancelMapTextEditor()
    {
        dataManager.Load(); // Revert to last saved version from json
        dataManager.DrawMapTexts();
        CloseTextEditor();
    }

    public void CloseTextEditor()
    {
        if (!viewOnlyMode)
        {
            mapTextEditorPanel.SetActive(false);
            colorPicker.Close();
        }

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

    private void HandleEscapePress()
    {
        if (activeText != null)
        {
            if (!viewOnlyMode)
            {
                CancelMapTextEditor();
                CancelTitleEditor();
            }
            return;
        }
        if (activeNode != null)
        {
            if (viewOnlyMode)
            {
                CloseEditor();
            }
            else
            {
                CancelNodeEditor();
            }
        }
    }
}