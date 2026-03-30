using UnityEngine;
using TMPro;
public class SliderUI : MonoBehaviour
{   
    [SerializeField] private TextMeshProUGUI sliderText = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SliderChanged(float value)
    {
        float localValue = value;
        sliderText.text = localValue.ToString("0");
    }
}
