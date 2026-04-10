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
    public GameObject noteButtonPrefab;
    public TMP_InputField noteEditor;
    public TMP_InputField titleEditor;

    [Header("Ribbon Tools")]
    public Color highlightColor = Color.yellow; //Default highlight color for ribbon tools

    [Header("Dynamic Map Settings")]
    public string currentMapName;

    private string folderPath;
    private string currentNotePath;

    void Start()
    {
        noteEditor.richText = true; //Allows rich text formatting

        SetMap(PlayerPrefs.GetString("LastMapFolder", ""));
        InitializeNotes();
        noteEditor.onEndEdit.AddListener(delegate { SaveNote(); });
        titleEditor.onEndEdit.AddListener(delegate { RenameNote(); });
    }

    public void FormatBold() => ApplyTag("<b>", "</b>");
    public void FormatItalic() => ApplyTag("<i>", "</i>");
    public void FormatUnderline() => ApplyTag("<u>", "</u>");
    public void FormatResize(float size) => ApplyTag($"<size={size}>", "</size>");

    public void FormatColor(Color color)
    {
        string hex = ColorUtility.ToHtmlStringRGB(color);
        ApplyTag($"<color=#{hex}>", "</color>");
    }

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
        noteEditor.selectionStringAnchorPosition = start + formattedText.Length;

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

        // Regex explanation: 
        // <[^>]*> matches anything starting with <, ending with >, 
        // and containing any characters except > in between.
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

        File.WriteAllText(fullPath, ""); // Create empty note file

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