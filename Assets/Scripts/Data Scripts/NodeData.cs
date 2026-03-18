[System.Serializable]
public class NodeData
{
    public float x;
    public float y;
    public float size;
    public string text;
    public string titleTextId;
    public int priority;
    public string type;

    public NodeData(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.size = 1.0f;
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
    public float yOffset;
    public float xOffset;
    public string content;

    public int priority;
    public float fontSize;
    public string colorHex;
    public float rotation;
    public float arc;

    public MapTextData() { }

    public MapTextData(float x, float y)
    {
        this.id = System.Guid.NewGuid().ToString();
        this.x = x;
        this.y = y;
        this.xOffset = 0f;
        this.yOffset = 0f;
        this.content = "New Text";
        this.priority = 0;
        this.fontSize = 14f;
        this.colorHex = "#FFFFFF";
        this.rotation = 0f;
        this.arc = 0f;
    }
}