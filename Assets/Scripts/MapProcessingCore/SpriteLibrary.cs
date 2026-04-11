using MapProcessing.Core;

public class SpriteLibrary
{
    // Dictionary or Array to hold our pre-scaled variants
    private Dictionary<int, ImageData[]> _mountainScales = new Dictionary<int, ImageData[]>();
    //private ImageData[] _treeVariants; // If we had multiple tree variants, we could do the same for them
    private ImageData _treeSprite;

    public float userMaxSize = 0.5f;

    public void Initialize(string folder)
    {
        _treeSprite = ImageLoader.Load(Path.Combine(folder, "tree1.png"));

        int[] ids = { 1, 2, 3, 4 };
        float[] scaleMultipliers = { 1.0f, 0.85f, 0.7f, 0.5f };

        foreach (int id in ids)
        {
            ImageData baseTex = ImageLoader.Load(Path.Combine(folder, $"mountain{id}.png"));

            // If userMaxSize is 2.0, our "1.0f" scale is actually double the file size
            _mountainScales[id] = new ImageData[4];
            for (int i = 0; i < 4; i++)
            {
                float finalScale = scaleMultipliers[i] * userMaxSize;
                _mountainScales[id][i] = SpriteResizer.Resize(baseTex, finalScale);
            }
        }
    }

    public ImageData GetMountain(int variant, float distance)
    {
        // Simple logic: pick the index (0-3) based on distance field value
        int scaleIdx = CalculateScaleIndex(distance);
        return _mountainScales[variant][scaleIdx];
    }

    public ImageData GetTree()
    {
        return _treeSprite;
    }

    private int CalculateScaleIndex(float distance)
    {
        // These numbers are pixel distances from the biome edge
        if (distance >= 35f) return 0; // 1.0x (Deep interior)
        if (distance >= 20f) return 1; // 0.85x
        if (distance >= 10f) return 2; // 0.7x
        return 3;                      // 0.5x (Thin edges)
    }
}