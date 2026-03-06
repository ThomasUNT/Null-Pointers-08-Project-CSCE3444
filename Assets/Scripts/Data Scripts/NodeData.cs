[System.Serializable]
public class NodeData
{
    public float x;
    public float y;
    public string text;
    public string titleTextId;
    public int priority;
    public string type;

    public NodeData(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.text = "";

        // reference to MapTextData entry for the title text of the node
        this.titleTextId = "";

        this.priority = 0;
        this.type = "default";
    }
}

[System.Serializable]
public class MapTextData
{
    public string id;
    public float x;
    public float y;
    public string content;

    public int priority;
    public float fontSize;
    public string colorHex;
    public float rotation;
    public float arc;
}