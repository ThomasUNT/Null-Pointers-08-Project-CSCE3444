using System.IO;
using UnityEngine;
using UnityEngine.UI;
using MapProcessing.Core;

public class MapMaskManager : MonoBehaviour
{
    public int width = 1536;
    public int height = 1024;
    public DisplayMaskToUI uiDisplay;

    public Texture2D maskTexture;
    private Texture2D displayTexture;
    private MapProcessor processor;

    private ImageData libMaskInput;
    private Color32[] resultPixels;

    private string filePath;

    void Start()
    {
        string folder = PlayerPrefs.GetString("LastMapFolder", "");

        if (string.IsNullOrEmpty(folder))
        {
            Debug.LogError("No map folder set.");
            return;
        }

        filePath = Path.Combine(folder, "colorMask.png");

        // Initialize MapProcessor
        string texPath = Path.Combine(Application.streamingAssetsPath, "MapTextures");
        processor = new MapProcessor();
        processor.Initialize(texPath, width, height);

        // Setup Buffers
        libMaskInput = new ImageData(width, height);
        resultPixels = new Color32[width * height];
        displayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        LoadOrCreateMask();

        SyncUnityToLib(maskTexture.GetPixels32(), libMaskInput);

        UpdateFinalMap();
    }

    public void UpdateLivePreview()
    {
        SyncUnityToLib(maskTexture.GetPixels32(), libMaskInput);

        // Process only the live preview
        ImageData result = processor.ProcessLive(libMaskInput);

        // Render result back to display
        SyncLibToUnity(result, resultPixels);
        displayTexture.SetPixels32(resultPixels);
        displayTexture.Apply();

        uiDisplay.SetDisplayTexture(displayTexture);
    }

    public void UpdateFinalMap()
    {
        ImageData final = processor.ProcessFinal(processor.ProcessLive(libMaskInput));

        SyncLibToUnity(final, resultPixels);
        displayTexture.SetPixels32(resultPixels);
        displayTexture.Apply();

        uiDisplay.SetDisplayTexture(displayTexture);
    }

    private void SyncUnityToLib(Color32[] source, ImageData dest)
    {
        for (int i = 0; i < source.Length; i++)
        {
            dest.Pixels[i].R = source[i].r;
            dest.Pixels[i].G = source[i].g;
            dest.Pixels[i].B = source[i].b;
            dest.Pixels[i].A = source[i].a;
        }
    }

    private void SyncLibToUnity(ImageData source, Color32[] dest)
    {
        for (int i = 0; i < dest.Length; i++)
        {
            dest[i].r = source.Pixels[i].R;
            dest[i].g = source.Pixels[i].G;
            dest[i].b = source.Pixels[i].B;
            dest[i].a = 255;
        }
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

        string directory = Path.GetDirectoryName(filePath);
        string mapPath = Path.Combine(directory, "map.png");

        byte[] mapData = displayTexture.EncodeToPNG();
        File.WriteAllBytes(mapPath, mapData);
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