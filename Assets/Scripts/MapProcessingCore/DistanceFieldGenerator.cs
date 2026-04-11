using System;
using System.Collections.Generic;
using MapProcessing.Core;

namespace MapProcessing.Core.Utils
{
    public static class DistanceFieldGenerator
    {
        public static void Generate(ImageData mask, float[,] dist)
        {
            int w = mask.Width;
            int h = mask.Height;
            float maxDist = w + h; // Large initial value

            // Initialize: 0 for edges, Infinity for everything else
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    dist[x, y] = IsEdge(mask, x, y) ? 0 : maxDist;
                }
            }

            // First Pass: Top-Left to Bottom-Right
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Check(dist, x, y, x - 1, y, 1, w, h);     // Left
                    Check(dist, x, y, x, y - 1, 1, w, h);     // Top
                    Check(dist, x, y, x - 1, y - 1, 1.41f, w, h); // Top-Left
                }
            }

            // Second Pass: Bottom-Right to Top-Left
            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = w - 1; x >= 0; x--)
                {
                    Check(dist, x, y, x + 1, y, 1, w, h);     // Right
                    Check(dist, x, y, x, y + 1, 1, w, h);     // Bottom
                    Check(dist, x, y, x + 1, y + 1, 1.41f, w, h); // Bottom-Right
                }
            }
        }

        public static void GenerateInternal(ImageData mask, float[,] outputBuffer)
        {
            int w = mask.Width;
            int h = mask.Height;
            float maxDist = w + h; // Large initial value

            // Initialize: 0 for edges, Infinity for everything else
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    outputBuffer[x, y] = IsBoundary(mask, x, y) ? 0 : maxDist;
                }
            }

            // First Pass: Top-Left to Bottom-Right
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Check(outputBuffer, x, y, x - 1, y, 1, w, h);     // Left
                    Check(outputBuffer, x, y, x, y - 1, 1, w, h);     // Top
                    Check(outputBuffer, x, y, x - 1, y - 1, 1.41f, w, h); // Top-Left
                }
            }

            // Second Pass: Bottom-Right to Top-Left
            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = w - 1; x >= 0; x--)
                {
                    Check(outputBuffer, x, y, x + 1, y, 1, w, h);     // Right
                    Check(outputBuffer, x, y, x, y + 1, 1, w, h);     // Bottom
                    Check(outputBuffer, x, y, x + 1, y + 1, 1.41f, w, h); // Bottom-Right
                }
            }
        }

        private static void Check(float[,] dist, int x, int y, int nx, int ny, float weight, int w, int h)
        {
            if (nx >= 0 && nx < w && ny >= 0 && ny < h)
            {
                float newDist = dist[nx, ny] + weight;
                if (newDist < dist[x, y]) dist[x, y] = newDist;
            }
        }

        private static bool IsEdge(ImageData mask, int x, int y)
        {
            int i = y * mask.Width + x;
            if (IsWater(mask.Pixels[i])) return false;

            // Check neighbors using offsets to avoid expensive math
            int w = mask.Width;
            int h = mask.Height;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    {
                        // Direct array access is much faster than GetPixel
                        if (IsWater(mask.Pixels[ny * w + nx])) return true;
                    }
                }
            }
            return false;
        }

        private static bool IsBoundary(ImageData mask, int x, int y)
        {
            int i = y * mask.Width + x;
            if (IsForest(mask.Pixels[i]))
            {
                // Check neighbors using offsets to avoid expensive math
                int w = mask.Width;
                int h = mask.Height;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        {
                            // Direct array access is much faster than GetPixel
                            if (!IsForest(mask.Pixels[ny * w + nx])) return true;
                        }
                    }
                }
            }
            else if (IsMountain(mask.Pixels[i]))
            {
                // Check neighbors using offsets to avoid expensive math
                int w = mask.Width;
                int h = mask.Height;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        {
                            // Direct array access is much faster than GetPixel
                            if (!IsMountain(mask.Pixels[ny * w + nx])) return true;
                        }
                    }
                }
            }
            return false;
        }
        private static bool IsWater(Pixel p) => p.R == 255 && p.G == 255 && p.B == 255;
        private static bool IsForest(Pixel p) => p.R == 0 && p.G == 255 && p.B == 0;
        private static bool IsMountain(Pixel p) => p.R == 255 && p.G == 0 && p.B == 0;
    }
}
