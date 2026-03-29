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
    public float fontName;
    public float mapScale;
    public float mapZoom;

    public MapSettings()
    {
        this.fontName = "";
        this.mapScale = 1.0f;
        this.mapZoom = 1.0f;
    }
}
