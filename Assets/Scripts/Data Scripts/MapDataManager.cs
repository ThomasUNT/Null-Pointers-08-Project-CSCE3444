using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class MapDataManager : MonoBehaviour
{
    public MapData mapData = new MapData();
    private string filePath;
    public RectTransform mapRect;
    public GameObject nodeIconPrefab;
    public GameObject mapTextPrefab;
    public NodeEditorUI editorUI;
    public int defaultNodeSizeMultiplier = 1;

    [Header("Node Type Sprites")]
    public Sprite ruinsSprite;
    public Sprite villageSprite;
    public Sprite townSprite;
    public Sprite citySprite;
    public Sprite majorCitySprite;
    public Sprite defaultSprite;


    List<GameObject> spawnedIcons = new List<GameObject>();
    List<GameObject> spawnedTexts = new List<GameObject>();

    void Start()
    {
        string folder = PlayerPrefs.GetString("LastMapFolder", "");

        if (string.IsNullOrEmpty(folder))
        {
            Debug.LogError("No active map folder set!");
            return;
        }

        filePath = Path.Combine(folder, "data.json");
        Load();

        // Wait a frame to ensure UI layout is ready
        StartCoroutine(DrawNodesNextFrame());
    }

    private IEnumerator DrawNodesNextFrame()
    {
        yield return null; // wait 1 frame
        DrawNodes();
        DrawMapTexts();
    }

    void Update()
    {
        // compensate for map scaling live
        float mapScale = mapRect.localScale.x;

        for (int i = 0; i < spawnedIcons.Count; i++)
        {
            var node = mapData.nodes[i];

            float finalScale = (defaultNodeSizeMultiplier * node.size) / mapScale;

            spawnedIcons[i].transform.localScale = Vector3.one * finalScale;
        }

        for (int i = 0; i < spawnedTexts.Count; i++)
        {
            var textObj = spawnedTexts[i];
            var textData = mapData.mapTexts[i];

            // Base normalized position
            float baseX = textData.x;
            float baseY = textData.y;

            // Node size if this text is attached to a node
            float nodeSize = 1f;
            NodeData attachedNode = mapData.nodes.Find(n => n.titleTextId == textData.id);
            if (attachedNode != null)
                nodeSize = attachedNode.size;

            // Scale offsets dynamically: offsets grow with node size and inversely with map scale
            float scaledXOffset = textData.xOffset * (nodeSize * 0.8f) / mapScale;
            float scaledYOffset = textData.yOffset * (nodeSize * 0.8f) / mapScale;

            // Convert to pixel coordinates
            Rect rect = mapRect.rect;
            float xPos = rect.x + (baseX * rect.width) + (scaledXOffset * rect.width);
            float yPos = rect.y + (baseY * rect.height) + (scaledYOffset * rect.height);

            textObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);

            // Keep text visually consistent while zooming
            textObj.transform.localScale = Vector3.one / mapScale;
        }
    }

    public void AddNode(Vector2 position)
    {
        NodeData node = new NodeData(position.x, position.y);
        mapData.nodes.Add(node);
        Save();
        DrawNodes();
    }

    private Sprite GetSpriteForType(string type)
    {
        switch (type)
        {
            case "Ruins": return ruinsSprite;
            case "Village": return villageSprite;
            case "Town": return townSprite;
            case "City": return citySprite;
            case "Major City": return majorCitySprite;
            default: return defaultSprite; ;
        }
    }

    public void DrawNodes()
    {
        // clear old icons
        foreach (var icon in spawnedIcons)
            Destroy(icon);

        spawnedIcons.Clear();

        for (int i = 0; i < mapData.nodes.Count; i++)
        {
            var node = mapData.nodes[i];

            GameObject icon = Instantiate(nodeIconPrefab, mapRect);

            Rect rect = mapRect.rect;

            float xPos = rect.x + node.x * rect.width;
            float yPos = rect.y + node.y * rect.height;

            icon.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(xPos, yPos);

            UnityEngine.UI.Image image = icon.GetComponent<UnityEngine.UI.Image>();
            image.sprite = GetSpriteForType(node.type);

            // compensate for map scaling
            float mapScale = mapRect.localScale.x;

            float finalScale = (defaultNodeSizeMultiplier * node.size) / mapScale;
            icon.transform.localScale = Vector3.one * finalScale;

            icon.GetComponent<NodeIcon>()
                .Initialize(i, editorUI);

            spawnedIcons.Add(icon);
        }
        DrawMapTexts();
    }

    public void DrawMapTexts()
    {
        // clear old texts
        foreach (var textObj in spawnedTexts)
            Destroy(textObj);
        
        spawnedTexts.Clear();

        Rect rect = mapRect.rect;

        for (int i = 0; i < mapData.mapTexts.Count; i++)
        {
            var textData = mapData.mapTexts[i];

            GameObject textObj = Instantiate(mapTextPrefab, mapRect);

            // Get normalized base position
            float finalNormalizedX = textData.x;
            float finalNormalizedY = textData.y;

            // Adjust offsets based on node size and map zoom
            float mapScale = mapRect.localScale.x; // <1 = zoomed out, icons bigger
            float nodeSize = 1f;

            // If this text is attached to a node, get its size
            NodeData attachedNode = mapData.nodes.Find(n => n.titleTextId == textData.id);
            if (attachedNode != null)
                nodeSize = attachedNode.size;

            // Multiply offsets by node size and by inverse of map scale
            float scaledXOffset = textData.xOffset * nodeSize / mapScale;
            float scaledYOffset = textData.yOffset * nodeSize / mapScale;

            // Convert to pixel space
            float xPos = rect.x + (finalNormalizedX * rect.width) + (scaledXOffset * rect.width);
            float yPos = rect.y + (finalNormalizedY * rect.height) + (scaledYOffset * rect.height);

            textObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);

            // Pull text
            TMPro.TMP_Text tmp = textObj.GetComponent<TMPro.TMP_Text>();
            tmp.text = textData.content;
            tmp.fontSize = textData.fontSize;

            // Rotation
            textObj.transform.localRotation = Quaternion.Euler(0,0, textData.rotation);

            // Arc
            CurvedText curved = textObj.GetComponent<CurvedText>();
            if (curved != null)
            {
                curved.UpdateCurve(textData.arc);
            }

            // compensate for map scaling
            textObj.transform.localScale = Vector3.one / mapScale;

            spawnedTexts.Add(textObj);
        }
    }


    public void Save()
    {
        string json = JsonUtility.ToJson(mapData, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Saved to: " + filePath);
    }


    public void Load()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            mapData = JsonUtility.FromJson<MapData>(json);
        }
    }
}