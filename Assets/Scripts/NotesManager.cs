using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class NotesManager : MonoBehaviour
{
    public Transform notesListContent;
    public GameObject noteButtonPrefab;

    public TMP_InputField noteEditor;

    private string folderPath;
    private string currentNotePath;

    void Start()
    {
        folderPath = Path.Combine(Application.persistentDataPath, "Notes");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        LoadNotes();
    }

    void LoadNotes()
    {
        foreach (Transform child in notesListContent)
            Destroy(child.gameObject);

        string[] files = Directory.GetFiles(folderPath, "*.txt");

        foreach (string file in files)
        {
            GameObject btn = Instantiate(noteButtonPrefab, notesListContent);

            string fileName = Path.GetFileNameWithoutExtension(file);

            btn.GetComponentInChildren<TMP_Text>().text = fileName;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                OpenNote(file);
            });
        }
    }

    public void CreateNote()
    {
        string noteName = "Note_" + System.DateTime.Now.Ticks;

        string path = Path.Combine(folderPath, noteName + ".txt");

        File.WriteAllText(path, "");

        LoadNotes();
    }

    void OpenNote(string path)
    {
        currentNotePath = path;

        noteEditor.text = File.ReadAllText(path);
    }

    public void DeleteNote()
    {
        if (currentNotePath == null) return;

        File.Delete(currentNotePath);

        currentNotePath = null;

        noteEditor.text = "";

        LoadNotes();
    }

    public void SaveNote()
    {
        if (currentNotePath == null) return;

        File.WriteAllText(currentNotePath, noteEditor.text);
    }
}