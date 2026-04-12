using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSavePreviewManager : MonoBehaviour
{
    public string GotoSceneOnSuccess;
    public GameObject PreviewImage;
    public TMPro.TMP_Text PreviewTitle;
    public TMPro.TMP_Text PreviewDetails;
    public GameObject PreviewUnselectedCover;

    private string curDisplayedPath = null;

    public void SetPreview(string savePath)
    {
        // Hide unselected cover (show preview under it)
        PreviewUnselectedCover.SetActive(false);

        if (!Directory.Exists(savePath))
        {
            Debug.LogError("Map not found at: " + savePath);
            PreviewUnselectedCover.SetActive(true);
            return;
        }

        curDisplayedPath = savePath;

        // Populate title
        PreviewTitle.text = savePath.Split(Path.DirectorySeparatorChar).Last();

        // Populate details
        string details = "Save size: ";
        long sizeBytes = DirSize(new DirectoryInfo(savePath));
        if (sizeBytes >= Math.Pow(1024, 3))
        {
            details += string.Format("{0:F1}", sizeBytes / Math.Pow(1024, 3)) + " GiB";
        }
        if (sizeBytes >= Math.Pow(1024, 2))
        {
            details += string.Format("{0:F1}", sizeBytes / Math.Pow(1024, 2)) + " MiB";
        }
        else if (sizeBytes >= 1024)
        {
            details += string.Format("{0:F1}", sizeBytes / 1024.0) + " KiB";
        }
        else
        {
            details += sizeBytes.ToString() + " B";
        }
        PreviewDetails.text = details;

        // Populate image
        string imagePath = FindMapImage(savePath);
        Texture2D tex = new Texture2D(1, 1);
        if (imagePath != null)
        {
            // Found a valid image
            byte[] fileData = File.ReadAllBytes(imagePath);
            tex.LoadImage(fileData);

        } else
        {
            // Did not find a valid image, need to set preview to gray
            tex.SetPixel(0, 0, new Color(0x80, 0x80, 0x80, 1));
        }
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
        Image img = PreviewImage.GetComponent<Image>();
        img.sprite = sprite;

        // Set aspect ratio of the fitter so the image is scaled correctly in the preview with different screen sizes
        AspectRatioFitter imageFitter = PreviewImage.GetComponent<AspectRatioFitter>();
        imageFitter.aspectRatio = tex.width / tex.height;
    }


    // Blanks out the save preview pane
    public void ResetPreview()
    {
        PreviewUnselectedCover.SetActive(true);
    }

    public void OpenMap()
    {
        Debug.Log("Opening folder " + curDisplayedPath);
        if (!Directory.Exists(curDisplayedPath))
        {
            Debug.LogError("Chosen map folder not found.");
            return;
        }

        PlayerPrefs.SetString("LastMapFolder", curDisplayedPath);
        PlayerPrefs.Save();

        SceneManager.LoadScene(GotoSceneOnSuccess);
    }


    // From LoadMapToUI.cs
    // Returns the path to the first image with a known image extension
    string FindMapImage(string folder)
    {
        string[] extensions = { "*.png", "*.jpg", "*.jpeg" };

        foreach (string ext in extensions)
        {
            string[] files = Directory.GetFiles(folder, ext);
            if (files.Length > 0)
            {
                return files[0]; // return first match
            }
        }

        return null;
    }


    // From https://stackoverflow.com/a/468131
    static long DirSize(DirectoryInfo d)
    {
        long size = 0;
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis)
        {
            size += fi.Length;
        }
        // Add subdirectory sizes.
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis)
        {
            size += DirSize(di);
        }
        return size;
    }
}
