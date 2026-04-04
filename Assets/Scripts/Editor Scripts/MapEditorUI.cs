using UnityEngine;
using TMPro;

public class MapEditorUI : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject biomesPanel;
    public GameObject buttonPanel;
    public GameObject leftPanel;

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
}
