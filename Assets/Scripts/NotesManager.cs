using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

public class NotesManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform notesListContent;
    public GameObject noteButtonPrefab;
    public TMP_InputField noteEditor;
    public TMP_InputField titleEditor;

    [Header("Dynamic Map Settings")]
    public string currentMapName;

    private string folderPath;
    private string currentNotePath;

    private string currentNoteId;
    private string currentNodeId;

    void Start()
    {
        SetMap(PlayerPrefs.GetString("LastMapFolder", ""));
        InitializeNotes();
        noteEditor.onEndEdit.AddListener(delegate { SaveNote(); });
        titleEditor.onEndEdit.AddListener(delegate { RenameNote(); });
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


        noteEditor.text = "";
        titleEditor.text = "";

        LoadNotes();
    }

    public void LoadNotes()
    {
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
        string content = BuildFrontmatter(id, "");

        File.WriteAllText(fullPath, content); // Store IDs in new note file

        currentNotePath = fullPath;
        currentNoteId = id;
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
        titleEditor.text = Path.GetFileNameWithoutExtension(path);
        string rawText = File.ReadAllText(path);
        var parsed = ParseNoteFile(rawText);

        currentNoteId = parsed.id;
        currentNodeId = parsed.nodeId;

        noteEditor.text = parsed.content;
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
        titleEditor.text = "";
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