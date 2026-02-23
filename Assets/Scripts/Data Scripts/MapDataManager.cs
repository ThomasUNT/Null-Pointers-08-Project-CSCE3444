using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class MapDataManager : MonoBehaviour
{
    public MapData mapData = new MapData();
    private string filePath;
    public RectTransform mapRect;
    public GameObject nodeIconPrefab;
    public NodeEditorUI editorUI;

    List<GameObject> spawnedIcons = new List<GameObject>();

    void Awake()
    {
        string folder = PlayerPrefs.GetString("LastMapFolder", "");

        if (string.IsNullOrEmpty(folder))
        {
            Debug.LogError("No active map folder set!");
            return;
        }

        filePath = Path.Combine(folder, "data.json");
        Load();
        DrawNodes();
    }

    void Update()
    {
        // compensate for map scaling live
        foreach (var icon in spawnedIcons)
        {
            icon.transform.localScale = Vector3.one / mapRect.localScale.x;
        }

    }

    public void AddNode(Vector2 position)
    {
        NodeData node = new NodeData(position.x, position.y);
        mapData.nodes.Add(node);
        Save();
        DrawNodes();
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

            // compensate for map scaling
            float mapScale = mapRect.localScale.x;
            icon.transform.localScale = Vector3.one / mapScale;

            icon.GetComponent<NodeIcon>()
                .Initialize(i, editorUI);

            spawnedIcons.Add(icon);
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