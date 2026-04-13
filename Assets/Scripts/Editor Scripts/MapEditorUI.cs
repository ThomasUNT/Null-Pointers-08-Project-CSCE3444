using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapEditorUI : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject startPanel;
    public GameObject biomesPanel;
    public GameObject buttonPanel;
    public GameObject leftPanel;
    public GameObject settingsPanel;
    public GameObject landSettingsPanel;
    public GameObject waterSettingsPanel;

    [Header("External References")]
    public MapDataManager dataManager;
    public MapMaskManager maskManager;

    [Header("General UI Components")]
    public SliderUI startPanelSlider;
    public SliderUI biomePanelSlider;

    [Header("Land Settings UI Components")]
    public Toggle shorelineToggle;
    public Slider shorelineDarknessSlider;
    public Slider shorelineWidthSlider;
    public Slider mountainSizeSlider;
    public Slider mountainDensitySlider;
    public Slider roughenScaleSlider;
    public Slider roughenStrengthSlider;

    [Header("Water Settings UI Components")]
    public Toggle waterDepthToggle;
    public Slider depthDistSlider;
    public Slider maxDepthSlider;
    public Toggle waveHighlightsToggle;
    public Toggle taperWavesToggle;
    public Slider waveBrightnessSlider;
    public Slider waveDistanceSlider;
    public Slider waveSpacingSlider;
    public Slider waveThicknessSlider;


    public void OpenBiomesPanel()
    {
        startPanel.SetActive(false);
        biomesPanel.SetActive(true);

        biomePanelSlider.RefreshFromSource();
    }

    public void OpenStartPanel()
    {
        startPanel.SetActive(true);
        biomesPanel.SetActive(false);
        settingsPanel.SetActive(false);

        startPanelSlider.RefreshFromSource();
    }

    public void OpenButtonPanel()
    {
        buttonPanel.SetActive(true);
        leftPanel.SetActive(false);
    }

    public void OpenLeftPanel()
    {
        buttonPanel.SetActive(false);
        leftPanel.SetActive(true);
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
        startPanel.SetActive(false);
        landSettingsPanel.SetActive(false);
        waterSettingsPanel.SetActive(false);
    }

    public void OpenLandSettingsPanel()
    {
        landSettingsPanel.SetActive(true);
        settingsPanel.SetActive(false);
        PopulateLandUI();
    }

    public void OpenWaterSettingsPanel()
    {
        waterSettingsPanel.SetActive(true);
        settingsPanel.SetActive(false);
        PopulateWaterUI();
    }

    private void PopulateLandUI()
    {
        MapSettings s = dataManager.mapData.settings;
        shorelineToggle.isOn = s.shorelineDarkening;
        shorelineDarknessSlider.value = s.shorelineDarkness;
        shorelineWidthSlider.value = s.shorelineWidth;
        roughenScaleSlider.value = s.roughenScale;
        roughenStrengthSlider.value = s.roughenStrength;
    }

    private void PopulateWaterUI()
    {
        MapSettings s = dataManager.mapData.settings;
        waterDepthToggle.isOn = s.waterDepth;
        depthDistSlider.value = s.waterDepthDistance;
        maxDepthSlider.value = s.maxDepthDarkness;
        waveHighlightsToggle.isOn = s.waveHighlights;
        taperWavesToggle.isOn = s.taperWaves;
        waveBrightnessSlider.value = s.waveBrightness;
        waveDistanceSlider.value = s.waveDistance;
        waveSpacingSlider.value = s.waveSpacing;
        waveThicknessSlider.value = s.waveThickness;
    }

    public void SaveAndApplySettings()
    {
        MapSettings s = dataManager.mapData.settings;

        // Land
        s.shorelineDarkening = shorelineToggle.isOn;
        s.shorelineDarkness = shorelineDarknessSlider.value;
        s.shorelineWidth = shorelineWidthSlider.value;
        s.roughenScale = roughenScaleSlider.value;
        s.roughenStrength = roughenStrengthSlider.value;

        // Water
        s.waterDepth = waterDepthToggle.isOn;
        s.waterDepthDistance = depthDistSlider.value;
        s.maxDepthDarkness = maxDepthSlider.value;
        s.waveHighlights = waveHighlightsToggle.isOn;
        s.taperWaves = taperWavesToggle.isOn;
        s.waveBrightness = waveBrightnessSlider.value;
        s.waveDistance = waveDistanceSlider.value;
        s.waveSpacing = waveSpacingSlider.value;
        s.waveThickness = waveThicknessSlider.value;

        // Save to JSON file
        dataManager.Save();

        maskManager.processor.ApplySettings(s);

        // Redraw the map with new settings
        maskManager.UpdateFinalMap();

        Debug.Log("Map Settings Saved and Redrawn.");
    }

    public void ApplyDefaultSettings()
    {
        dataManager.mapData.settings = new MapSettings();
        MapSettings s = dataManager.mapData.settings;

        dataManager.Save();

        maskManager.processor.ApplySettings(s);

        maskManager.UpdateFinalMap();

        Debug.Log("Settings reset to defaults and saved.");
    }
}
