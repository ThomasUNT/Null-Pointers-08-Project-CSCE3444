[System.Serializable]
public class NodeData
{
    public float x;
    public float y;
    public string text;
    public string title;
    public int priority;
    public string type;

    public NodeData(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.text = "";
        this.title = "";
        this.priority = 0;
        this.type = "default";
    }
}