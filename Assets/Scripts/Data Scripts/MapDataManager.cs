using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class MapDataManager : MonoBehaviour
{
    public bool suppressUI = false;

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

    /*    float maxZoom = 2.0f; // TEMP placeholder
        float normalizedZoom = mapRect.localScale.x / maxZoom;*/


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
    }

    public NodeData AddNode(Vector2 position)
    {
        NodeData node = new NodeData(position.x, position.y);
        mapData.nodes.Add(node);
        //Save(); 
        DrawNodes();

        return node;
    }

    public MapTextData AddText(Vector2 position)
    {
        MapTextData textData = new MapTextData(position.x, position.y);
        mapData.mapTexts.Add(textData);
        //Save();
        DrawMapTexts();

        // adding a return to prevent errors in the editor when trying to access the newest text's font before it's fully initialized
        return textData;
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
        if (suppressUI) return;

        float mapScale = mapRect.localScale.x;

        // TEMPORARY
        float minZoom = 1f;
        float maxZoom = 3f;
        float normalizedZoom = Mathf.Clamp01((mapScale - minZoom) / (maxZoom - minZoom));

        Debug.Log("MapScale: " + mapRect.localScale.x);

        // clear old icons
        foreach (var icon in spawnedIcons)
            DestroyImmediate(icon);

        spawnedIcons.Clear();

        foreach (var node in mapData.nodes)
        {
            GameObject icon = Instantiate(nodeIconPrefab, mapRect);

            Rect rect = mapRect.rect;

            float xPos = rect.x + node.x * rect.width;
            float yPos = rect.y + node.y * rect.height;

            icon.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(xPos, yPos);

            UnityEngine.UI.Image image = icon.GetComponent<UnityEngine.UI.Image>();
            image.sprite = GetSpriteForType(node.type);

            // compensate for map scaling
            float finalScale = (defaultNodeSizeMultiplier * node.size) / mapScale;
            icon.transform.localScale = Vector3.one * finalScale;

            icon.GetComponent<NodeIcon>()
                .Initialize(node, editorUI);

            icon.SetActive(ShouldRender(node.priority, normalizedZoom));

            spawnedIcons.Add(icon);
        }
        DrawMapTexts();
    }

    public void DrawMapTexts()
    {
        if (suppressUI) return;

        float mapScale = mapRect.localScale.x;
        float minZoom = 1f;
        float maxZoom = 3f;
        float normalizedZoom = Mathf.Clamp01((mapScale - minZoom) / (maxZoom - minZoom));

        // clear old texts
        foreach (var textObj in spawnedTexts)
            DestroyImmediate(textObj);

        spawnedTexts.Clear();

        Rect rect = mapRect.rect;

        foreach (var textData in mapData.mapTexts)
        {
            GameObject textObj = Instantiate(mapTextPrefab, mapRect);

            textObj.GetComponent<MapTextIcon>()
            .Initialize(textData, editorUI, mapData);

            // Get normalized base position
            float finalNormalizedX = textData.x;
            float finalNormalizedY = textData.y;

            // Adjust offsets based on node size and map zoom
            //float mapScale = mapRect.localScale.x; // <1 = zoomed out, icons bigger
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
            textObj.transform.localRotation = Quaternion.Euler(0, 0, textData.rotation);

            // Arc
            CurvedText curved = textObj.GetComponent<CurvedText>();
            if (curved != null)
            {
                curved.UpdateCurve(textData.arc);
            }

            // compensate for map scaling
            textObj.transform.localScale = Vector3.one / mapScale;

            textObj.SetActive(ShouldRender(textData.priority, normalizedZoom));

            spawnedTexts.Add(textObj);
        }
    }

    // Temporary - Placeholder thresholds until we implement user-set map settings
    private float[] priorityThresholds = new float[]
    {
        0f, // always
        0f, // always
        0.1f, // any zoom
        0.5f, // mid zoom
        0.9f // max zoom
    };

    private bool ShouldRender(int priority, float normalizedZoom)
    {
        if (priority < 0 || priority >= priorityThresholds.Length)
            return true;

        return normalizedZoom >= priorityThresholds[priority];
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