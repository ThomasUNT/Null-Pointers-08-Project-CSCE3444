using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenMapButton : MonoBehaviour
{
    public void OpenMap(string GotoSceneOnSuccess)
    {
        // Get map root folder path (and create a folder there if one does not already exist)
        string mapsRoot = Path.Combine(Application.persistentDataPath, "maps");
        if (!Directory.Exists(mapsRoot))
            Directory.CreateDirectory(mapsRoot);

        var chosenPaths = StandaloneFileBrowser.OpenFolderPanel(
            "Select Map Folder",
            mapsRoot,
            false
        );

        if (chosenPaths.Length == 0)
        {
            Debug.Log("Cancelled folder open operation");
            return;
        }

        // This is the final data path
        string chosenPath = chosenPaths[0];

        if (!Directory.Exists(chosenPath))
        {
            Debug.LogError("Chosen map folder not found.");
            return;
        }

        // TODO: Need to check if map path is valid

        // Save 
        PlayerPrefs.SetString("LastMapFolder", chosenPath);
        PlayerPrefs.Save();

        // Load next scene
        SceneManager.LoadScene(GotoSceneOnSuccess);
    }
}
