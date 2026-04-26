using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ColorPickerPanel : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [Header("Sliders")]
    public Slider hueSlider;
    public Slider satSlider;
    public Slider valSlider;

    [Header("Visuals")]
    public Image previewImage;
    public Image satOverlayImage; // The transparent-to-solid gradient layer
    public Image valOverlayImage; // The transparent-to-solid gradient

    private RectTransform rectTransform;
    public Action<Color> onColorChangedCallback;
    private bool isSettingUp = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        hueSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        satSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        valSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
    }

    public void Initialize(Color initialColor, Action<Color> callback)
    {
        isSettingUp = true;
        this.onColorChangedCallback = callback;

        // Convert initial color to HSV to set slider positions
        Color.RGBToHSV(initialColor, out float h, out float s, out float v);
        hueSlider.value = h;
        satSlider.value = s;
        valSlider.value = v;

        isSettingUp = false;
        UpdateUI();

        PositionAtMouse();
        gameObject.SetActive(true);
    }

    public void UpdateUI()
    {
        float h = hueSlider.value;
        float s = satSlider.value;
        float v = valSlider.value;

        // Calculate the final color
        Color finalColor = Color.HSVToRGB(h, s, v);
        previewImage.color = finalColor;

        // Update Saturation Spectrum
        satOverlayImage.color = Color.HSVToRGB(h, 1, v);

        // Update Brightness Spectrum
        valOverlayImage.color = Color.HSVToRGB(h, s, 1);

        if (!isSettingUp && onColorChangedCallback != null)
        {
            onColorChangedCallback.Invoke(finalColor);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
    }

    private void PositionAtMouse()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        Vector2 mousePos = Input.mousePosition;
        RectTransform parentRect = transform.parent as RectTransform;
        
        if (parentRect != null)
        {
            // Converts the screen mouse position to a point relative to the UI Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                mousePos,
                null, // Use your UI Camera here if using World Space/Camera Overlay
                out Vector2 localPoint
            );

            // Add an offset so the panel doesn't spawn directly under the cursor 
            Vector2 offset = new Vector2(0, -200);
            rectTransform.anchoredPosition = localPoint + offset;
        }
    }

    public void Close()
    {
        onColorChangedCallback = null;
        gameObject.SetActive(false);
    }
}