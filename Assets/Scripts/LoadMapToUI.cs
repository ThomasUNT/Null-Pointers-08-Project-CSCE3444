using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class LoadMapToUI : MonoBehaviour
{
    void Start()
    {
        string folder = PlayerPrefs.GetString("LastMapFolder", "");

        if (string.IsNullOrEmpty(folder))
        {
            Debug.LogError("No map folder saved.");
            return;
        }

        string path = Path.Combine(folder, "map.png");

        if (!File.Exists(path))
        {
            Debug.LogError("Map not found at: " + path);
            return;
        }

        StartCoroutine(LoadImage(path));
    }


    IEnumerator LoadImage(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);

        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        Image img = GetComponent<Image>();
        img.sprite = sprite;

        // Fit image to window while maintaining aspect ratio
        RectTransform windowRT = transform.parent.GetComponent<RectTransform>();
        float windowWidth = windowRT.rect.width;
        float windowHeight = windowRT.rect.height;

        float scaleX = windowWidth / tex.width;
        float scaleY = windowHeight / tex.height;

        float scale = Mathf.Min(scaleX, scaleY);

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(tex.width * scale, tex.height * scale);
        rt.anchoredPosition = Vector2.zero;

        yield break;
    }
}
