using UnityEngine;
using System.IO;
using SFB;
using System.Data;

public class ImportMapButton : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public string sceneToLoad = "AddInfoNodes";

    public void ImportMap()
    {
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Map Image",
            "",
            extensions,
            false
        );

        if (paths.Length == 0 || paths[0] == "")
            return;

        string sourcePath = paths[0];

        if (!File.Exists(sourcePath))
        {
            Debug.LogError("File not found.");
            return;
        }

        // Root maps directory
        string mapsRoot = Path.Combine(Application.persistentDataPath, "maps");

        if (!Directory.Exists(mapsRoot))
            Directory.CreateDirectory(mapsRoot);

        // Find next available map folder name
        int mapIndex = 0;
        string mapFolder;

        do
        {
            mapFolder = Path.Combine(mapsRoot, "map" + mapIndex);
            mapIndex++;
        }
        while (Directory.Exists(mapFolder));

        // Create map folder
        Directory.CreateDirectory(mapFolder);

        // Copy image and convert to PNG for consistency
        byte[] fileData = File.ReadAllBytes(sourcePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        byte[] fileDataPNG = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(Path.Combine(mapFolder, "map.png"), fileDataPNG);

        Debug.Log("Map imported to: " + mapFolder);

        // Create starter JSON file
        string jsonPath = Path.Combine(mapFolder, "data.json");

        if (!File.Exists(jsonPath))
        {
            File.WriteAllText(jsonPath, "{ \"nodes\": [] }");
        }

        // Save folder path for later use
        PlayerPrefs.SetString("LastMapFolder", mapFolder);
        PlayerPrefs.Save();

        sceneLoader.LoadScene(sceneToLoad);
    }
}
