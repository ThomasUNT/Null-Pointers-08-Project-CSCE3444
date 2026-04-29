using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Loading;
using UnityEngine;
using UnityEngine.UI;

public class NotesManager : MonoBehaviour
{
    public MapDataManager dataManager;

    [Header("UI References")]
    public Transform notesListContent;
    public Transform nodeNotesListContent;
    public GameObject noteButtonPrefab;
    public TMP_InputField noteEditor;
    public TMP_InputField titleEditor;

    [Header("Ribbon Tools")]
    public Color highlightColor = Color.yellow; //Default highlight color for ribbon tools

    [Header("Dynamic Map Settings")]
    public string currentMapName;

    private string folderPath;
    private string currentNotePath;

    private string currentNoteId;
    private string currentNodeId;

    [Header("Font Settings")]
    public float sizeStep = 50f; //Step size for font resizing
    public float defaultBaseSize = 24f; //Default font size for new notes
    

    public void IncrementalIncrease() => AdjustSelectionSize(sizeStep);
    public void IncrementalDecrease() => AdjustSelectionSize(-sizeStep);

    private string selectedFolderPath;

    private void AdjustSelectionSize(float amount)
    {
        int start = Mathf.Min(noteEditor.selectionStringAnchorPosition, noteEditor.selectionStringFocusPosition);
        int end = Mathf.Max(noteEditor.selectionStringAnchorPosition, noteEditor.selectionStringFocusPosition);

        if (start == end) return; //No text selected to adjust

        string originalText = noteEditor.text;
        string selectedText = originalText.Substring(start, end - start);
        float currentSize = defaultBaseSize;

        Match match = Regex.Match(selectedText, @"<size=([\d\.]+)>");

        if (match.Success)
        {
            float.TryParse(match.Groups[1].Value, out currentSize);
        }

        float targetSize = currentSize + amount;

        string cleanedText = Regex.Replace(selectedText, @"<size=[\d\.]+>|<\/size>", string.Empty);

        string formattedText = $"<size={targetSize}>{cleanedText}</size>";

        noteEditor.text = originalText.Remove(start, end - start).Insert(start, formattedText); //Apply new size

        noteEditor.ActivateInputField();
        noteEditor.selectionStringAnchorPosition = start;
        noteEditor.selectionStringFocusPosition = start + formattedText.Length;

        SaveNote(); //Auto-save after resizing
    }

    void Start()
    {
        if (noteEditor != null)
        {
            noteEditor.richText = true; //Allows rich text formatting
            noteEditor.onEndEdit.AddListener(delegate { SaveNote(); });
        }

        if (titleEditor != null)
        {
            titleEditor.onEndEdit.RemoveAllListeners();
            titleEditor.onEndEdit.AddListener(delegate { OnTitleEndEdit(); });
        }

        SetMap(PlayerPrefs.GetString("LastMapFolder", ""));
        InitializeNotes();
    }

    public void FormatBold() => ApplyTag("<b>", "</b>");
    public void FormatItalic() => ApplyTag("<i>", "</i>");
    public void FormatUnderline() => ApplyTag("<u>", "</u>");
    public void FormatResize(float size) => ApplyTag($"<size={size}>", "</size>");



    private void ApplyTag(string open, string close)
    {
        int start = Mathf.Min(noteEditor.selectionStringAnchorPosition, noteEditor.selectionStringFocusPosition);
        int end = Mathf.Max(noteEditor.selectionStringAnchorPosition, noteEditor.selectionStringFocusPosition);

        if (start == end) return; //No highlighted text

        string originalText = noteEditor.text;
        string selectedText = originalText.Substring(start, end - start);
        string formattedText;

        if (selectedText.StartsWith(open) && selectedText.EndsWith(close))
        {
            formattedText = selectedText.Substring(open.Length, selectedText.Length - (open.Length + close.Length));
        }
        else
        {
            formattedText = $"{open}{selectedText}{close}";
        }

        noteEditor.text = originalText.Remove(start, end - start).Insert(start, formattedText); //Replace with formatted text

        noteEditor.ActivateInputField();

        noteEditor.selectionStringAnchorPosition = start;
        noteEditor.selectionStringFocusPosition = start + formattedText.Length;

        SaveNote(); //Auto-save after formatting
    }

    //CLEAR FORMATTING BUTTON
    public void ClearSelectedFormatting()
    {
        int start = Mathf.Min(noteEditor.selectionStringAnchorPosition, noteEditor.selectionStringFocusPosition);
        int end = Mathf.Max(noteEditor.selectionStringAnchorPosition, noteEditor.selectionStringFocusPosition);

        if (start == end) return; // No text selected to clear

        string text = noteEditor.text;
        string selectedText = text.Substring(start, end - start);

        string plainText = Regex.Replace(selectedText, "<[^>]*>", string.Empty);

        noteEditor.text = text.Remove(start, end - start).Insert(start, plainText);

        // Refocus and keep the now-plain text highlighted
        noteEditor.ActivateInputField();
        noteEditor.selectionStringAnchorPosition = start;
        noteEditor.selectionStringFocusPosition = start + plainText.Length;

        SaveNote();
    }

    public void SetMap(string newMapName)
    {
        currentMapName = newMapName;
        InitializeNotes();
    }

    private void InitializeNotes()
    {
        string basePersistentPath = Application.persistentDataPath;
        folderPath = Path.Combine(basePersistentPath, "maps", currentMapName, "Notes");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (noteEditor != null) noteEditor.text = "";
        if (titleEditor != null) titleEditor.text = "";

        NoteRegistry.Rebuild(folderPath);

        if (dataManager != null)
        {
            dataManager.ValidateNodeNotes();
        }

        if (notesListContent != null) LoadHierarchicalNotes();
    }

    private void GenerateTreeUI(string currentPath, Transform parentUI, int indentLevel)
    {
        // 1. Handle Subdirectories (Folders) first
        string[] subDirs = Directory.GetDirectories(currentPath);
        foreach (string dir in subDirs)
        {
            GameObject folderObj = Instantiate(noteButtonPrefab, parentUI);
            folderObj.GetComponentInChildren<TMP_Text>().text = new string(' ', indentLevel * 4) + Path.GetFileName(dir);

            UnityEngine.UI.Image folderImage = folderObj.GetComponent<UnityEngine.UI.Image>();
            if (folderImage != null)
            {
                folderImage.color = new Color(1f, 0.98f, 0.85f); //Folder color
            }


            // This container is where the folder's internal notes will live
            GameObject childContainer = new GameObject("ChildContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            childContainer.transform.SetParent(parentUI, false);

            var vlg = childContainer.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;

            var csf = childContainer.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // Don't let it shrink width
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


            childContainer.transform.SetAsLastSibling();

            Button btn = folderObj.GetComponent<Button>();
            btn.onClick.AddListener(() => {
                childContainer.SetActive(!childContainer.activeSelf);
                selectedFolderPath = dir;
                currentNotePath = "";
                if (titleEditor != null) titleEditor.text = Path.GetFileName(dir);
            });

            // RECURSION: This is the key. By passing childContainer.transform, 
            // the NEXT call to this function will put its notes inside THIS folder.
            GenerateTreeUI(dir, childContainer.transform, indentLevel + 1);
        }

        // 2. Handle Files (Notes)
        string[] files = Directory.GetFiles(currentPath, "*.md");
        foreach (string file in files)
        {
            // Use parentUI here. If we are inside a subfolder, parentUI IS the childContainer.
            GameObject btn = Instantiate(noteButtonPrefab, parentUI);
            string fileName = Path.GetFileNameWithoutExtension(file);

            var text = btn.GetComponentInChildren<TMP_Text>();
            text.text = new string(' ', indentLevel * 4) + fileName;

            string path = file;
            btn.GetComponent<Button>().onClick.AddListener(() => OpenNote(path));
        }
    }

    public void LoadHierarchicalNotes()
    {
        if (notesListContent == null) return;

        foreach (Transform child in notesListContent)
            Destroy(child.gameObject);

        if (!Directory.Exists(folderPath)) return;

        GenerateTreeUI(folderPath, notesListContent, 0);
    }

    public void LoadNotesByList(List<string> noteIds)
    {
        if (nodeNotesListContent == null) return;

        // Clear old buttons
        foreach (Transform child in nodeNotesListContent)
            Destroy(child.gameObject);

        if (noteIds == null || noteIds.Count == 0) return;

        // Loop through the specific IDs this node owns
        foreach (string id in noteIds)
        {
            string path = NoteRegistry.GetPath(id);

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                // Create the button
                GameObject btn = Instantiate(noteButtonPrefab, nodeNotesListContent);

                var renameScript = btn.GetComponent<RenameableNoteButton>();

                string fileName = Path.GetFileNameWithoutExtension(path);

                renameScript.Initialize(id, fileName, this);
            }
            else
            {
                Debug.LogWarning($"Note ID {id} found in Node, but path is missing from Registry/Disk.");
            }
        }
    }

    public string CreateNote(string nodeId)
    {
        if (string.IsNullOrEmpty(folderPath)) InitializeNotes();

        string targetDir = string.IsNullOrEmpty(selectedFolderPath) ? folderPath : selectedFolderPath;

        string finalName;
        string fullPath = GetUniquePath(targetDir, "note", out finalName);

        string id = System.Guid.NewGuid().ToString();
        string content = BuildFrontmatter(id, nodeId);

        File.WriteAllText(fullPath, content);
        NoteRegistry.UpdateEntry(id, fullPath);

        currentNotePath = fullPath;
        currentNoteId = id;
        currentNodeId = nodeId;

        LoadHierarchicalNotes();
        OpenNote(fullPath);
        return id;
    }

    public void CreateFolder(string folderName = "New Folder")
    {
        string newFolderPath = Path.Combine(folderPath, folderName);

        //Ensure unique folder name if it exists
        int counter = 1;
        while (Directory.Exists(newFolderPath))
        {
            newFolderPath = Path.Combine(folderPath, $"{folderName} {counter}");
            counter++;
        }

        Directory.CreateDirectory(newFolderPath);
        LoadHierarchicalNotes();
    }


    private string GetUniquePath(string folder, string baseName, out string finalName)
    {
        string fileName = baseName;
        string fullPath = Path.Combine(folder, fileName + ".md");
        int counter = 1;

        // Check if the file already exists. If so, append digit
        while (File.Exists(fullPath))
        {
            fileName = $"{baseName} {counter}";
            fullPath = Path.Combine(folder, fileName + ".md");
            counter++;
        }

        finalName = fileName;
        return fullPath;
    }

    public void CreateNoteFromButton()
    {
        CreateNote("");
    }

    void OpenNote(string path)
    {
        currentNotePath = path;

        if (titleEditor != null)
        {
            titleEditor.text = Path.GetFileNameWithoutExtension(path);
        }

        string rawText = File.ReadAllText(path);
        var parsed = ParseNoteFile(rawText);

        currentNoteId = parsed.id;
        currentNodeId = parsed.nodeId;

        noteEditor.text = parsed.content;
    }

    public void OpenNoteById(string noteId)
    {
        string path = NoteRegistry.GetPath(noteId);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            OpenNote(path);
        }
        else
        {
            Debug.LogError($"Note with ID {noteId} not found in Registry!");
        }
    }

    public string GetCurrentNoteId()
    {
        return currentNoteId;
    }

    public void SaveNote()
    {
        if (string.IsNullOrEmpty(currentNotePath)) return;

        string frontmatter = BuildFrontmatter(currentNoteId, currentNodeId);

        string fullFileContent = frontmatter + "\n" + noteEditor.text;

        File.WriteAllText(currentNotePath, fullFileContent);
    }

    public void RenameNote()
    {
        if (string.IsNullOrEmpty(currentNotePath)) return;

        string desiredName = titleEditor.text;

        // If the name hasn't actually changed, don't do anything
        if (Path.GetFileNameWithoutExtension(currentNotePath) == desiredName) return;

        string finalName;
        string currentDirectory = Path.GetDirectoryName(currentNotePath);
        string newPath = GetUniquePath(currentDirectory, desiredName, out finalName);

        File.Move(currentNotePath, newPath);
        currentNotePath = newPath;

        // Update UI to match the final filename
        titleEditor.text = finalName;

        NoteRegistry.UpdateEntry(currentNoteId, newPath);
        LoadHierarchicalNotes();
    }

    public string RenameNoteById(string id, string newName)
    {
        string oldPath = NoteRegistry.GetPath(id);
        if (string.IsNullOrEmpty(oldPath) || !File.Exists(oldPath)) return newName;

        // If the name hasn't changed, just return
        if (Path.GetFileNameWithoutExtension(oldPath) == newName) return newName;

        string folder = Path.GetDirectoryName(oldPath);
        string finalName;
        string newPath = GetUniquePath(folder, newName, out finalName);

        File.Move(oldPath, newPath);
        NoteRegistry.UpdateEntry(id, newPath);

        if (currentNoteId == id)
        {
            currentNotePath = newPath;
            if (titleEditor != null) titleEditor.text = finalName;
        }

        return finalName; // Return the name (possibly with digit appended)
    }

    public void OnTitleEndEdit()
    {
        // If a note is open, use your existing note rename logic
        if (!string.IsNullOrEmpty(currentNotePath))
        {
            RenameNote();
        }
        // If no note is open but a folder is selected, use the folder rename logic
        else if (!string.IsNullOrEmpty(selectedFolderPath) && selectedFolderPath != folderPath)
        {
            RenameFolder();
        }
    }

    public void RenameFolder()
    {
        if (string.IsNullOrEmpty(selectedFolderPath)) return;

        string desiredName = titleEditor.text;
        if (Path.GetFileName(selectedFolderPath) == desiredName) return;

        string finalName;
        string parentDir = Path.GetDirectoryName(selectedFolderPath);

        // Use a folder-specific version of your unique path helper
        string newPath = GetUniqueFolderPath(parentDir, desiredName, out finalName);

        try
        {
            Directory.Move(selectedFolderPath, newPath);
            selectedFolderPath = newPath;
            titleEditor.text = finalName;

            LoadHierarchicalNotes(); // Refresh the sidebar
        }
        catch (System.Exception e)
        {
            Debug.LogError("Folder Rename Failed: " + e.Message);
        }
    }

    private string GetUniqueFolderPath(string parent, string baseName, out string finalName)
    {
        string folderName = baseName;
        string fullPath = Path.Combine(parent, folderName);
        int counter = 1;

        while (Directory.Exists(fullPath))
        {
            folderName = $"{baseName} {counter}";
            fullPath = Path.Combine(parent, folderName);
            counter++;
        }

        finalName = folderName;
        return fullPath;
    }

    public void DeleteNote()
    {
        if (string.IsNullOrEmpty(currentNotePath)) return;
        if (File.Exists(currentNotePath))
        {
            File.Delete(currentNotePath);
            Debug.Log("Deleted note at: " + currentNotePath);
        }

        ClearEditorUI();
        LoadHierarchicalNotes();
    }

    public void DeleteFolder()
    {
        // Ensure we aren't trying to delete the root "Notes" folder
        if (string.IsNullOrEmpty(selectedFolderPath) || selectedFolderPath == folderPath)
        {
            Debug.LogWarning("Cannot delete the root directory or no folder selected.");
            return;
        }

        if (Directory.Exists(selectedFolderPath))
        {
            // true means it will delete all files and subfolders inside
            Directory.Delete(selectedFolderPath, true);

            // Reset selection to root so the next note doesn't try to save to a deleted path
            selectedFolderPath = folderPath;


            LoadHierarchicalNotes();
            ClearEditorUI();

            Debug.Log("Folder deleted successfully.");
        }
    }

    public void ClearEditorUI()
    {
        if (noteEditor != null) noteEditor.text = "";
        if (titleEditor != null) titleEditor.text = "";
        currentNoteId = "";
        currentNotePath = "";
    }

    private string BuildFrontmatter(string id, string nodeId)
    {
        return
        "---\nid: " + id + "\nnodeId: " + nodeId + "\n---";
    }

    private (string id, string nodeId, string content) ParseNoteFile(string text)
    {
        if (!text.StartsWith("---"))
            return (null, null, text);

        int end = text.IndexOf("---", 3);
        if (end == -1)
            return (null, null, text);

        string frontmatter = text.Substring(3, end - 3);
        string content = text.Substring(end + 3).TrimStart();

        string id = null;
        string nodeId = null;

        foreach (var line in frontmatter.Split('\n'))
        {
            var parts = line.Split(':');
            if (parts.Length < 2) continue;

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            if (key == "id") id = value;
            if (key == "nodeId") nodeId = value;
        }

        return (id, nodeId, content);
    }
}