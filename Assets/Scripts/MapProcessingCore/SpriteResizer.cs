using MapProcessing.Core;
using System;

public static class SpriteResizer
{
    public static ImageData Resize(ImageData original, float scale)
    {
        int newWidth = (int)(original.Width * scale);
        int newHeight = (int)(original.Height * scale);

        // Ensure at least 1x1
        newWidth = Math.Max(1, newWidth);
        newHeight = Math.Max(1, newHeight);

        ImageData result = new ImageData(newWidth, newHeight);
        Pixel[] origPix = original.Pixels;
        Pixel[] resPix = result.Pixels;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // Map new coordinates back to original coordinates
                float gx = ((float)x / newWidth) * (original.Width - 1);
                float gy = ((float)y / newHeight) * (original.Height - 1);

                int gxi = (int)gx;
                int gyi = (int)gy;

                // Simple Bilinear Interpolation
                resPix[y * newWidth + x] = origPix[gyi * original.Width + gxi];
            }
        }
        return result;
    }
}