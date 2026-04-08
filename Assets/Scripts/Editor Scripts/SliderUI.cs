using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderUI : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI sliderText = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private MapDrawHandler drawHandler = null;

    void Start()
    {
        slider.SetValueWithoutNotify(drawHandler.brushSize);
        UpdateText(slider.value);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SliderChanged(float value)
    {
        drawHandler.SetBrushSize(value);
        UpdateText(value);
    }

    public void RefreshFromSource()
    {
        // Pull latest value from DrawHandler
        float value = drawHandler.brushSize;

        slider.SetValueWithoutNotify(value);
        UpdateText(value);
    }

    void UpdateText(float value)
    {
        sliderText.text = value.ToString("0");
    }
}