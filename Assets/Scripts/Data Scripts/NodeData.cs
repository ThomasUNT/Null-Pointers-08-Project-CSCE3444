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

[System.Serializable]
public class MapSettings
{
    // --- Water Settings ---
    // depth darkening settings
    public bool waterDepth;
    public float waterDepthDistance;
    public float maxDepthDarkness;

    // wave settings
    public bool waveHighlights;
    public bool taperWaves;
    public float waveBrightness;
    public float waveDistance;
    public float waveSpacing;
    public float waveThickness;

    // --- Land Settings ---
    // shoreline darkening settings
    public bool shorelineDarkening;
    public float shorelineDarkness;
    public float shorelineWidth;

    // mountain settings
    public float mountainSize;
    public float mountainDensity;

    // roughener settings
    public float roughenScale;
    public float roughenStrength;

    public MapSettings()
    {
        // Default settings
        waterDepth = true;
        waterDepthDistance = 300f;
        maxDepthDarkness = 0.8f;

        waveHighlights = true;
        taperWaves = true;
        waveBrightness = 40f;
        waveDistance = 30f;
        waveSpacing = 8f;
        waveThickness = 3f;

        shorelineDarkening = true;
        shorelineDarkness = 0.5f;
        shorelineWidth = 5f;

        mountainSize = 1f;
        mountainDensity = 0.5f;

        roughenScale = 0.03f;
        roughenStrength = 18f;
    }
}