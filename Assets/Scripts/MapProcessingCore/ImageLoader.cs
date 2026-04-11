using UnityEngine;
using System.IO;
using MapProcessing.Core;

namespace MapProcessing.Core.Utils
{
    public static class ImageLoader
    {
        public static ImageData Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[ImageLoader] File not found at: {path}");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(path);

            // Unity's Texture2D uses lowercase 'width' and 'height'
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(fileData))
            {
                Debug.LogError($"[ImageLoader] Failed to decode image at: {path}");
                return null;
            }

            ImageData data = new ImageData(tex.width, tex.height);
            Color32[] unityPixels = tex.GetPixels32();

            for (int i = 0; i < unityPixels.Length; i++)
            {
                data.Pixels[i].R = unityPixels[i].r;
                data.Pixels[i].G = unityPixels[i].g;
                data.Pixels[i].B = unityPixels[i].b;
                data.Pixels[i].A = unityPixels[i].a;
            }

            Object.DestroyImmediate(tex);
            return data;
        }

        public static void Save(ImageData data, string path)
        {
            // Unity's Texture2D constructor takes lowercase parameters
            Texture2D tex = new Texture2D(data.Width, data.Height, TextureFormat.RGBA32, false);
            Color32[] unityPixels = new Color32[data.Pixels.Length];

            for (int i = 0; i < unityPixels.Length; i++)
            {
                unityPixels[i].r = data.Pixels[i].R;
                unityPixels[i].g = data.Pixels[i].G;
                unityPixels[i].b = data.Pixels[i].B;
                unityPixels[i].a = data.Pixels[i].A;
            }

            tex.SetPixels32(unityPixels);
            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(path, pngData);

            Object.DestroyImmediate(tex);
        }
    }
}