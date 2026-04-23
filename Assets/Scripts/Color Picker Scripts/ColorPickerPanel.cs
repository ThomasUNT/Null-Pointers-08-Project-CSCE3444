using UnityEngine;
using UnityEngine.UI;

public class ColorPickerPanel : MonoBehaviour
{
    [Header("Sliders")]
    public Slider hueSlider;
    public Slider satSlider;
    public Slider valSlider;

    [Header("Visuals")]
    public Image previewImage;
    public Image satOverlayImage; // The transparent-to-solid gradient layer
    public Image valOverlayImage; // The transparent-to-solid gradient layer

    void Start()
    {
        // Ensure sliders are set to 0-1 in the Inspector!
        hueSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        satSlider.onValueChanged.AddListener(delegate { UpdateUI(); });
        valSlider.onValueChanged.AddListener(delegate { UpdateUI(); });

        UpdateUI();
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
    }
}