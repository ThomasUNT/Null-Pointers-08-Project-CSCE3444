using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RenameableNoteButton : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public TMP_Text labelText;
    public TMP_InputField editField;

    private string noteId;
    private NotesManager manager;
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    public void Initialize(string id, string name, NotesManager notesManager)
    {
        noteId = id;
        manager = notesManager;
        labelText.text = name;
        editField.text = name;

        editField.gameObject.SetActive(false); // Start hidden

        // Handle when user finishes typing
        editField.onEndEdit.AddListener(OnFinishRename);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastClick = Time.time - lastClickTime;

        if (timeSinceLastClick <= doubleClickThreshold)
        {
            StartRenaming();
        }
        else
        {
            // Normal click: Just open the note
            manager.OpenNoteById(noteId);
        }

        lastClickTime = Time.time;
    }

    private void StartRenaming()
    {
        editField.gameObject.SetActive(true);
        editField.Select();
        editField.ActivateInputField();
        labelText.gameObject.SetActive(false);
    }

    private void OnFinishRename(string newName)
    {
        // Avoid renaming to empty
        if (!string.IsNullOrEmpty(newName) && newName != labelText.text)
        {
            string actualName = manager.RenameNoteById(noteId, newName);

            // Update the label to display any appended digits to file name to avoid duplicates
            labelText.text = newName;
            editField.text = actualName;
        }

        editField.gameObject.SetActive(false);
        labelText.gameObject.SetActive(true);
    }
}