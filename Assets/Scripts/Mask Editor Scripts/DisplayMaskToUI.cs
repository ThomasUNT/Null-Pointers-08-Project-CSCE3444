using UnityEngine;
using UnityEngine.UI;

public class DisplayMaskToUI : MonoBehaviour
{
    public MapMaskManager maskManager;

    void Start()
    {
        if (maskManager == null || maskManager.maskTexture == null)
        {
            Debug.LogError("MaskManager or maskTexture missing.");
            return;
        }

        ApplyTexture(maskManager.maskTexture);
    }

    public void ApplyTexture(Texture2D tex)
    {
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );

        Image img = GetComponent<Image>();
        img.sprite = sprite;

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

        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(tex.width * scale, tex.height * scale);
        rt.anchoredPosition = Vector2.zero;
    }
}