using UnityEngine;
using UnityEngine.UI;

public class DisplayMaskToUI : MonoBehaviour
{
    private RawImage _rawImage;
    private RectTransform _rt;

    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        _rt = GetComponent<RectTransform>();
        if (_rawImage == null)
        {
            Debug.LogError("RawImage component missing.");
        }
    }

    public void SetDisplayTexture(Texture2D tex)
    {
        if (_rawImage == null) return;
        _rawImage.texture = tex;
        FitToParent(tex);
    }

    void FitToParent(Texture2D tex)
    {
        RectTransform windowRT = transform.parent.GetComponent<RectTransform>();
        float windowWidth = windowRT.rect.width;
        float windowHeight = windowRT.rect.height;

        float scaleX = windowWidth / tex.width;
        float scaleY = windowHeight / tex.height;

        float scale = Mathf.Min(scaleX, scaleY);

        _rt.sizeDelta = new Vector2(tex.width * scale, tex.height * scale);
        _rt.anchoredPosition = Vector2.zero;
    }
}