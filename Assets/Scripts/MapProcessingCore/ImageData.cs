using System;

namespace MapProcessing.Core
{
    public struct Pixel
    {
        public byte R, G, B, A;
    }

    public class ImageData
    {
        public int Width { get; }
        public int Height { get; }
        public Pixel[] Pixels { get; }

        public ImageData(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Pixel[width * height];
        }

        public ImageData Clone()
        {
            ImageData copy = new ImageData(Width, Height);
            Array.Copy(Pixels, copy.Pixels, Pixels.Length);
            return copy;
        }

        public Pixel GetPixel(int x, int y) => Pixels[y * Width + x];
        public void SetPixel(int x, int y, Pixel p) => Pixels[y * Width + x] = p;
    }
}
