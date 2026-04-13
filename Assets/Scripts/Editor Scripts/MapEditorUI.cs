using UnityEngine;
using TMPro;

public class MapEditorUI : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject biomesPanel;
    public GameObject buttonPanel;
    public GameObject leftPanel;
    public GameObject settingsPanel;
    public GameObject landSettingsPanel;
    public GameObject waterSettingsPanel;

    public SliderUI startPanelSlider;
    public SliderUI biomePanelSlider;

    public MapDataManager dataManager;


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
    }

    public void OpenWaterSettingsPanel()
    {
        waterSettingsPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
}
