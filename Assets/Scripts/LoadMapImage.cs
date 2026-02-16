using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LoadMapImage : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        string path = PlayerPrefs.GetString("LastMapPath", "");

        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(LoadImage(path));
        }
        else
        {
            Debug.LogError("No saved map path found.");
        }
    }

    IEnumerator LoadImage(string path)
    {
        string fileURL = "file://" + path;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fileURL);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        else
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );

            spriteRenderer.sprite = sprite;

            Debug.Log("Texture size: " + tex.width + " x " + tex.height);
            Debug.Log("Sprite bounds: " + sprite.bounds.size);


            //FitToMapArea(tex.width, tex.height);
        }
    }

    void FitToMapArea(float width, float height)
    {
        float mapAreaPercent = 0.7f;

        float worldScreenHeight = Camera.main.orthographicSize * 2;
        float worldScreenWidth = worldScreenHeight * Screen.width / Screen.height;

        float mapAreaWidth = worldScreenWidth * mapAreaPercent;

        float imageRatio = width / height;
        float mapAreaRatio = mapAreaWidth / worldScreenHeight;

        Vector3 scale = transform.localScale;

        if (imageRatio >= mapAreaRatio)
        {
            scale.x = mapAreaWidth;
            scale.y = mapAreaWidth / imageRatio;
        }
        else
        {
            scale.y = worldScreenHeight;
            scale.x = worldScreenHeight * imageRatio;
        }

        transform.localScale = scale;

        // shift map into right-side map area
        float leftUIWidth = worldScreenWidth * (1 - mapAreaPercent);

        transform.position = new Vector3(
            leftUIWidth / 2f,
            0,
            0
        );
    }
}
