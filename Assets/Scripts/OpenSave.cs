using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenSave : MonoBehaviour
{
    public MapSavePreviewManager PreviewManager;
    public TMPro.TMP_Text OwnText;

    private string pathToOpen;

    public void SetActiveMap()
    {
        PreviewManager.SetPreview(pathToOpen);
    }

    public void SetPath(string path)
    {
        pathToOpen = path;
        OwnText.text = Path.GetFileName(pathToOpen);
    }
}
