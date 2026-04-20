using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Rendering;
using System.Text.RegularExpressions;

public class NotesManager : MonoBehaviour
{
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
            titleEditor.onEndEdit.AddListener(delegate { RenameNote(); });
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

        if (notesListContent != null) LoadNotes();
    }

    public void LoadNotes()
    {
        if (notesListContent == null) return;

        foreach (Transform child in notesListContent)
            Destroy(child.gameObject);

        if (!Directory.Exists(folderPath)) return;

        string[] files = Directory.GetFiles(folderPath, "*.md");

        foreach (string file in files)
        {
            GameObject btn = Instantiate(noteButtonPrefab, notesListContent);
            string fileName = Path.GetFileNameWithoutExtension(file);
            btn.GetComponentInChildren<TMP_Text>().text = fileName;

            string path = file;
            btn.GetComponent<Button>().onClick.AddListener(() => OpenNote(path));
        }
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

                string fileName = Path.GetFileNameWithoutExtension(path);
                btn.GetComponentInChildren<TMP_Text>().text = fileName;

                // Hook up the click event
                btn.GetComponent<Button>().onClick.AddListener(() => OpenNote(path));
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

        int noteIndex = 0;
        string fileName;
        string fullPath;

        do
        {
            fileName = "note" + noteIndex;
            fullPath = Path.Combine(folderPath, fileName + ".md");
            noteIndex++;
        }
        while (File.Exists(fullPath));

        string id = System.Guid.NewGuid().ToString();
        string content = BuildFrontmatter(id, nodeId);

        File.WriteAllText(fullPath, content); // Store IDs in new note file

        NoteRegistry.UpdateEntry(id, fullPath);

        currentNotePath = fullPath;
        currentNoteId = id;
        currentNodeId = nodeId;

        LoadNotes();
        OpenNote(fullPath);
        return id;
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

        string newPath = Path.Combine(folderPath, titleEditor.text + ".md");

        if (currentNotePath != newPath && !File.Exists(newPath))
        {
            File.Move(currentNotePath, newPath);
            currentNotePath = newPath;

            // Update the registry with the new path
            NoteRegistry.UpdateEntry(currentNoteId, newPath);
            LoadNotes();
        }
    }

    public void DeleteNote()
    {
        if (string.IsNullOrEmpty(currentNotePath)) return;
        if (File.Exists(currentNotePath))
        {
            File.Delete(currentNotePath);
        }

        noteEditor.text = "";

        if (titleEditor != null)
        {
            titleEditor.text = "";
        }

        currentNoteId = null;
        currentNodeId = null;
        LoadNotes();
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