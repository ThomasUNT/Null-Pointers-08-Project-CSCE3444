using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public List<NodeData> nodes = new List<NodeData>();
    public List<MapTextData> mapTexts = new List<MapTextData>();
}
[System.Serializable]