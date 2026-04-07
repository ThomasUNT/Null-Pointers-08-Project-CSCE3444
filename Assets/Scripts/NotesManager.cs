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

    public void CreateNote()
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


        
        currentNotePath = fullPath;
        LoadNotes();
        OpenNote(fullPath);
    }

    void OpenNote(string path)
    {
        currentNotePath = path;
        titleEditor.text = Path.GetFileNameWithoutExtension(path);
        noteEditor.text = File.ReadAllText(path);
    }

    public void SaveNote()
    {
        if (string.IsNullOrEmpty(currentNotePath)) return;
        File.WriteAllText(currentNotePath, noteEditor.text);
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
        LoadNotes();
    }
}