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
        string folder = PlayerPrefs.GetString("LastMapFolder", "");

        if (string.IsNullOrEmpty(folder))
        {
            Debug.LogError("No map folder set.");
            return;
        }

        filePath = Path.Combine(folder, "colorMask.png");

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

        maskTexture.filterMode = FilterMode.Point;
        maskTexture.anisoLevel = 0;
        maskTexture.wrapMode = TextureWrapMode.Clamp;

        Color32[] pixels = new Color32[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white; // water default
        }

        maskTexture.SetPixels32(pixels);
        maskTexture.Apply();
    }

    public void SaveMask()
    {
        byte[] pngData = maskTexture.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
    }

    void LoadMask()
    {
        byte[] fileData = File.ReadAllBytes(filePath);

        maskTexture = new Texture2D(2, 2);

        maskTexture.filterMode = FilterMode.Point;
        maskTexture.anisoLevel = 0;
        maskTexture.wrapMode = TextureWrapMode.Clamp;

        maskTexture.LoadImage(fileData);
    }
}

/*public static class MapBridge
{
    // Unity -> Library
    public static void UpdateLibraryFromUnity(Color32[] unityPixels, ImageData libData)
    {
        for (int i = 0; i < unityPixels.Length; i++)
        {
            libData.Pixels[i].R = unityPixels[i].r;
            libData.Pixels[i].G = unityPixels[i].g;
            libData.Pixels[i].B = unityPixels[i].b;
            libData.Pixels[i].A = unityPixels[i].a;
        }
    }

    // Library -> Unity
    public static void UpdateUnityFromLibrary(ImageData libData, Color32[] unityPixels)
    {
        for (int i = 0; i < unityPixels.Length; i++)
        {
            unityPixels[i].r = libData.Pixels[i].R;
            unityPixels[i].g = libData.Pixels[i].G;
            unityPixels[i].b = libData.Pixels[i].B;
            unityPixels[i].a = libData.Pixels[i].A;
        }
    }
}*/