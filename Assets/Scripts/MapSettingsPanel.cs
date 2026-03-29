using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MapSettingsPanel : MonoBehaviour{
   [Header("Panel Reference")]
   [SerializeField] private GameObject mapSettingsPanel;
   [SerializeField] private GameObject buttonPanel;

   [Header("Font Settings")]
   [SerializeField] private TMP_Dropdown fontDropdown;

   [Header("Map Sliders")]
   [SerializeField] private Slider mapScaleSlider;
   [SerializeField] private Slider mapZoomSlider;
   [SerializeField] private TMP_Text mapScaleValueText;
   [SerializeField] private TMP_Text mapZoomValueText;

   [Header("Dependencies")]
   [SerializeField] private MapDataManager dataManager;


  private List<TMP_FontAsset> availableFonts = new List<TMP_FontAsset>();
  private int selectedFontIndex = 0;

 void Start(){
  LoadFontsFromResources();
  PopulateFontDropdown();

   if(mapScaleSlider != null)
     mapScaleSlider.onValueChanged.AddListener(v => {
          if (mapScaleValueText != null)
              mapScaleValueText.text = v.ToString("F2");
    });
   if (mapZoomSlider != null)
    mapZoomSlider.onValueChanged.AddListener(v => {
          if (mapZoomValueText != null)
              mapZoomValueText.text = v.ToString("F2");
    });
 
}

private void LoadFontsFromResources(){
  availableFonts.Clear();
  TMP_FontAsset[] fonts = Resources.LoadAll<TMP_FontAsset>("Fonts");

  foreach (var font in fonts)
  {
    if (font != null)
      availableFonts.Add(font);
  }
    if (availableFonts.Count == 0)
       Debug.LogWarning("MapSettingsPanel: No fonts found in Resources/Fonts/." + "Make sure your .asset font fies are in Assets/Resources/Fonts/");
    else
       Debug.Log($"MapSettingsPanel: Loaded {availableFonts.Count} fonts.");
}

private void PopulateFontDropdown()
{
  if (fontDropdown == null) return;
  
   fontDropdown.ClearOptions();
 
   List<string> fontNames = new List<string>();
   foreach (var font in availableFonts)
    fontNames.Add(font.name);

   fontDropdown.AddOptions(fontNames);
   fontDropdown.onValueChanged.AddListener(OnFontSelected);

   string savedFont = dataManager.mapData.mapSettings.fontName;
   int savedIndex = availableFonts.FindIndex(f => f.name == savedFont);
   if (savedIndex >= 0)
       fontDropdown.SetValueWithoutNotify(savedIndex);
}
private void OnFontSelected(int index)
{
  if (index < 0 || index >= availableFonts.Count) return;

   selectedFontIndex = index;
   TMP_FontAsset chosenFont = availableFonts[index];
   dataManager.mapData.mapSettings.fontName = chosenFont.name;
   ApplyFontToAllMapTexts(chosenFont);
   
   Debug.Log($"Font changed to: {chosenFont.name}");
}
private void ApplyFontToAllMapTexts(TMP_FontAsset font)
{
  foreach (var textObj in dataManager.spawnedTexts)
  {
     if (textObj == null)continue;
     TMP_Text tmp = textObj.GetComponent<TMP_Text>();
     if(tmp != null)
        tmp.font = font;
  }
}
public void OpenMapSettings()
{
  mapSettingsPanel.SetActive(true);
  buttonPanel.SetActive(false);
  PopulateFromMemory();

}
public void CloseMapSettings()
{
 mapSettingsPanel.SetActive(false);
 buttonPanel.SetActive(true);
  }
public void PopulateFromMemory()
{
   MapSettings settings = dataManager.mapData.mapSettings;
   if (mapScaleSlider != null)
   {
      mapScaleSlider.SetValueWithoutNotify(settings.mapScale);
      if (mapScaleValueText != null)
         mapScaleValueText.text = settings.mapScale.ToString("F2");
   }
   if (mapZoomSlider != null)
   {
      mapZoomSlider.SetValueWithoutNotify(settings.mapZoom);
      if (mapZoomValueText != null)
         mapZoomValueText.text = settings.mapZoom.ToString("F2");
   }
   if (fontDropdown != null)
   {
      int savedIndex = availableFonts.FindIndex(f => f.name == settings.fontName);
      if (savedIndex >= 0)
         fontDropdown.SetValueWithoutNotify(savedIndex);
    }
  }
}
