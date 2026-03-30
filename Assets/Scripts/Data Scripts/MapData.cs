using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public List<NodeData> nodes = new List<NodeData>();
    public List<MapTextData> mapTexts = new List<MapTextData>();
    public MapSettings mapSettings = new MapSettings();
}
[System.Serializable]
public class MapSettings
{
    public string fontName;
    public float mapScale;
    public float mapZoom;

    public MapSettings()
    {
        this.fontName = "";
        this.mapScale = 0.5f;
        this.mapZoom = 1f;
    }
}
