using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenSave : MonoBehaviour
{
    public string GotoSceneOnSuccess;
    public TMPro.TMP_Text ownText;

    private string pathToOpen;

    public void OpenSaveFolder()
    {
        Debug.Log("Opening folder " + pathToOpen);
        if (!Directory.Exists(pathToOpen))
        {
            Debug.LogError("Chosen map folder not found.");
            return;
        }

        PlayerPrefs.SetString("LastMapFolder", pathToOpen);
        PlayerPrefs.Save();

        SceneManager.LoadScene(GotoSceneOnSuccess);
    }

    public void SetPath(string path)
    {
        pathToOpen = path;
        ownText.text = Path.GetFileName(pathToOpen);
    }
}
