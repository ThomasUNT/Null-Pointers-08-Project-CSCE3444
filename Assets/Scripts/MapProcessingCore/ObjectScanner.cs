using System.Collections.Generic;
using MapProcessing.Core;

public class ObjectScanner
{
    public List<MapObject> TopTrees = new List<MapObject>();
    public List<MapObject> DepthObjects = new List<MapObject>();

    public float mountainScale = 1.0f;
    public float mountainDensity = 0.0f;

    public void Scan(ImageData mask, float[,] internalDist)
    {
        TopTrees.Clear();
        DepthObjects.Clear();

        int w = mask.Width;
        int h = mask.Height;
        Pixel[] pixels = mask.Pixels;

        // Dynamic spacing: If scale is 0.5, mountains spawn every 12 pixels. 
        // If scale is 1.0, they spawn every 25 pixels.
        int mSpacing = (int)(25 * mountainScale);
        if (mSpacing < 8) mSpacing = 8; // Safety cap

        for (int y = 0; y < h; y++)
        {
            int rowOffset = y * w;
            for (int x = 0; x < w; x++)
            {
                Pixel p = pixels[rowOffset + x];
                float d = internalDist[x, y];

                // --- FOREST LOGIC ---
                if (IsForest(p))
                {
                    // 1. Core Top/Bottom checks
                    bool isTop = (y == h - 1) || !IsForest(pixels[(y + 1) * w + x]);
                    bool isBottom = (y == 0) || !IsForest(pixels[(y - 1) * w + x]);

                    // 2. Identify "True Vertical" Sides
                    bool isSide = false;
                    if (!isTop && !isBottom) // Only check if it's not already a horizontal edge
                    {
                        // Must have forest above and below to be a "wall"
                        bool hasVerticalContinuity = (y > 0 && IsForest(pixels[(y - 1) * w + x])) &&
                        (y < h - 1 && IsForest(pixels[(y + 1) * w + x]));

                        if (hasVerticalContinuity)
                        {
                            // Check Left Void (Top-Left, Left, Bottom-Left)
                            bool leftVoid = (x == 0) ||
                                (y > 0 && !IsForest(pixels[(y - 1) * w + (x - 1)])) &&
                                (!IsForest(pixels[y * w + (x - 1)])) &&
                                (y < h - 1 && !IsForest(pixels[(y + 1) * w + (x - 1)]));

                            // Check Right Void (Top-Right, Right, Bottom-Right)
                            bool rightVoid = (x == w - 1) ||
                                (y > 0 && !IsForest(pixels[(y - 1) * w + (x + 1)])) &&
                                (!IsForest(pixels[y * w + (x + 1)])) &&
                                (y < h - 1 && !IsForest(pixels[(y + 1) * w + (x + 1)]));

                            isSide = leftVoid || rightVoid;
                        }
                    }

                    // 3. PLACEMENT LOGIC
                    // Use x % 5 for horizontals, and y % 5 for verticals to ensure 
                    // vertical walls get trees even if their X coordinate doesn't hit the modulo.
                    if (isTop && x % 7 == 0)
                    {
                        TopTrees.Add(new MapObject { X = x, Y = y });
                    }
                    else if ((isBottom && x % 7 == 0) || (isSide && y % 7 == 0))
                    {
                        DepthObjects.Add(new MapObject
                        {
                            X = x,
                            Y = y,
                            IsMountain = false
                        });
                    }
                }
                // --- MOUNTAIN LOGIC ---
                else if (IsMountain(p))
                {
                    // 1. Define the cell size based on scale (e.g., 20-30 pixels)
                    int cellSize = (int)((30 - (10 * mountainDensity)) * mountainScale);
                    if (cellSize < 10) cellSize = 10;

                    // 2. Find which "Cell" this pixel belongs to
                    int cellX = x / cellSize;
                    int cellY = y / cellSize;

                    // 3. Only attempt to spawn ONCE per cell. 
                    // We'll pick the pixel that matches our "Jitter" coordinate.
                    if (IsJitterPoint(x, y, cellX, cellY, cellSize))
                    {
                        // Use the cell coordinates for the variant so the mountain 
                        // doesn't change type if it jitters slightly.
                        int variant = 1 + ((cellX ^ cellY) % 4);

                        DepthObjects.Add(new MapObject
                        {
                            X = x,
                            Y = y,
                            IsMountain = true,
                            InternalDistance = d,
                            VariantID = variant
                        });
                    }
                }
            }
        }

        // Keep the Y-Sort for correct overlapping
        DepthObjects.Sort((a, b) => b.Y.CompareTo(a.Y));
    }

    private bool IsJitterPoint(int x, int y, int cellX, int cellY, int cellSize)
    {
        // Generate a pseudo-random offset for this specific cell
        uint hashX = (uint)(cellX * 73856093 ^ cellY * 19349663);
        uint hashY = (uint)(cellX * 83492791 ^ cellY * 53856093);

        int offsetX = (int)(hashX % cellSize);
        int offsetY = (int)(hashY % cellSize);

        // Only return true if the current pixel matches the jittered spot
        return (x % cellSize == offsetX) && (y % cellSize == offsetY);
    }

    private bool IsForest(Pixel p) => p.R == 0 && p.G == 255 && p.B == 0;
    private bool IsMountain(Pixel p) => p.R == 255 && p.G == 0 && p.B == 0;
}