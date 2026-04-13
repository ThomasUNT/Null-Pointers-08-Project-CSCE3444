using System;
using MapProcessing.Core;

public class EdgeRoughener
{
    public int Seed = 12345;

    // Adjustable Values
    public float Scale = 0.03f;
    public float Strength = 18.0f;
    public int Octaves = 3;

    private int[] _lookupTable;
    private int _cachedWidth, _cachedHeight;

    public void PrecomputeWarp(int width, int height)
    {
        Console.WriteLine($"Precomputing Warp Table for {width}x{height}...");
        _lookupTable = new int[width * height];
        _cachedWidth = width;
        _cachedHeight = height;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float offsetX = GetFractalNoise(x * Scale, y * Scale, Seed) * Strength;
                float offsetY = GetFractalNoise(x * Scale, y * Scale, Seed + 1) * Strength;

                int sampleX = (int)Math.Clamp(x + offsetX, 0, width - 1);
                int sampleY = (int)Math.Clamp(y + offsetY, 0, height - 1);

                _lookupTable[y * width + x] = sampleY * width + sampleX;
            }
        }
    }

    public void Process(ImageData input, ImageData output)
    {
        int width = input.Width;
        int height = input.Height;

        if (_lookupTable == null || width != _cachedWidth || height != _cachedHeight)
        {
            PrecomputeWarp(width, height);
        }

        Pixel[] inPix = input.Pixels;
        Pixel[] outpix = output.Pixels;

        for (int i = 0; i < outpix.Length; i++)
        {
            int sampleIndex = _lookupTable[i];
            outpix[i] = inPix[sampleIndex];
        }
    }

    private float GetFractalNoise(float x, float y, int seed)
    {
        float result = 0;
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float maxVal = 0;

        for (int i = 0; i < Octaves; i++)
        {
            result += SimpleSmoothNoise(x * frequency, y * frequency, seed + i) * amplitude;
            maxVal += amplitude;
            amplitude *= 0.5f;
            frequency *= 2.0f;
        }

        return result / maxVal;
    }

    private float SimpleSmoothNoise(float x, float y, int s)
    {
        int x0 = (int)Math.Floor(x);
        int x1 = x0 + 1;
        int y0 = (int)Math.Floor(y);
        int y1 = y0 + 1;

        float tx = x - x0;
        float ty = y - y0;

        float sx = tx * tx * (3 - 2 * tx);
        float sy = ty * ty * (3 - 2 * ty);

        float n0 = Hash(x0, y0, s);
        float n1 = Hash(x1, y0, s);
        float n2 = Hash(x0, y1, s);
        float n3 = Hash(x1, y1, s);

        float ix0 = Lerp(n0, n1, sx);
        float ix1 = Lerp(n2, n3, sx);

        return Lerp(ix0, ix1, sy);
    }

    private float Hash(int x, int y, int s)
    {
        unchecked
        {
            int n = x * 374761393 + y * 668265263 + s * 12345;
            n = (n ^ (n >> 13)) * 1274126177;
            return (float)((n ^ (n >> 16)) & 0x7fffffff) / 0x7fffffff * 2.0f - 1.0f;
        }
    }

    private float Lerp(float a, float b, float t) => a + t * (b - a);
}