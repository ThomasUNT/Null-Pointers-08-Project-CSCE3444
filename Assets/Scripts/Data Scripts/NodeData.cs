[System.Serializable]
public class NodeData
{
    public float x;
    public float y;
    public string text;

    public NodeData(float x, float y)
    {
        this.x = x;
        this.y = y;
        this.text = "";
    }
}