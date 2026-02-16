using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class LoadMapToUI : MonoBehaviour
{
    void Start()
    {
        string path = PlayerPrefs.GetString("LastMapPath", "");

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError("Map not found.");
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

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(tex.width, tex.height);

        yield break;
    }
}
