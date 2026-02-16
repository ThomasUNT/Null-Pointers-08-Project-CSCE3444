using UnityEngine;
using System.IO;
using SFB;

public class ImportMapButton : MonoBehaviour
{
    public void ImportMap()
    {
        // Open file browser
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Map Image",
            "",
            "png,jpg,jpeg",
            false
        );

        // If user cancels, stop
        if (paths.Length == 0 || paths[0] == "")
            return;

        string sourcePath = paths[0];

        if (!File.Exists(sourcePath))
        {
            Debug.LogError("File not found.");
            return;
        }

        // Create images folder in persistent storage
        string imagesFolder = Path.Combine(Application.persistentDataPath, "images");

        if (!Directory.Exists(imagesFolder))
            Directory.CreateDirectory(imagesFolder);

        // Copy image into app folder
        string fileName = Path.GetFileName(sourcePath);
        string destinationPath = Path.Combine(imagesFolder, fileName);

        File.Copy(sourcePath, destinationPath, true);

        Debug.Log("Map imported to: " + destinationPath);
    }
}
