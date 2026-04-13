using System.Collections.Generic;
using System.IO;
using MapProcessing.Core;
using MapProcessing.Core.Utils;

public class SpriteLibrary
{
    private Dictionary<int, ImageData[]> _mountainScales = new Dictionary<int, ImageData[]>();
    private ImageData _treeSprite;
    private string _lastFolder;
    private List<ImageData> _baseMountainTextures = new List<ImageData>();

    public void Initialize(string folder, float currentScale)
    {
        _lastFolder = folder;
        _treeSprite = ImageLoader.Load(Path.Combine(folder, "tree1.png"));

        // Load base textures once so we don't hit the disk every time the scale changes
        for (int id = 1; id <= 4; id++)
        {
            _baseMountainTextures.Add(ImageLoader.Load(Path.Combine(folder, $"mountain{id}.png")));
        }
    }

    public void UpdateScale(float newScale)
    {
        float[] scaleMultipliers = { 1.0f, 0.85f, 0.7f, 0.5f };

        for (int id = 1; id <= 4; id++)
        {
            ImageData baseTex = _baseMountainTextures[id - 1];
            _mountainScales[id] = new ImageData[4];

            for (int i = 0; i < 4; i++)
            {
                float finalScale = scaleMultipliers[i] * newScale;
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