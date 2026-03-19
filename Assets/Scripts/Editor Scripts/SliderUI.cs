using UnityEngine;
using TMPro;
public class SliderUI : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI sliderText = null;
    [SerializeField] private float maxSliderValue = 100.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SliderChanged(float value)
    {
        float localValue = value * maxSliderValue;
        sliderText.text = localValue.ToString("0");
    }
}
