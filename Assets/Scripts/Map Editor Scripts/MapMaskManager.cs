using System.IO;
using UnityEngine;

public class MapMaskManager : MonoBehaviour
{
    public int width = 1536;
    public int height = 1024;

    public Texture2D maskTexture;

    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "colorMask.png");
        LoadOrCreateMask();
    }

    void LoadOrCreateMask()
    {
        if (File.Exists(filePath))
        {
            LoadMask();
        }
        else
        {
            CreateBlankMask();
            SaveMask();
        }
    }

    void CreateBlankMask()
    {
        maskTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color32[] pixels = new Color32[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white; // water default
        }

        maskTexture.SetPixels32(pixels);
        maskTexture.Apply();
    }

    void SaveMask()
    {
        byte[] pngData = maskTexture.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
    }

    void LoadMask()
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        maskTexture = new Texture2D(2, 2);
        maskTexture.LoadImage(fileData);
    }
}