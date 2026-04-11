using MapProcessing.Core;

public static class SpriteStamper
{
    public static void Stamp(ImageData map, ImageData sprite, int startX, int startY)
    {
        Pixel[] mapPix = map.Pixels;
        Pixel[] sprPix = sprite.Pixels;
        int mapW = map.Width;
        int mapH = map.Height;
        int sprW = sprite.Width;
        int sprH = sprite.Height;

        for (int y = 0; y < sprH; y++)
        {
            int mapY = startY + y;
            if (mapY < 0 || mapY >= mapH) continue;

            int sprRow = y * sprW;
            int mapRow = mapY * mapW;

            for (int x = 0; x < sprW; x++)
            {
                int mapX = startX + x;
                if (mapX < 0 || mapX >= mapW) continue;

                Pixel s = sprPix[sprRow + x];
                if (s.A == 0) continue; // Skip fully transparent pixels

                if (s.A == 255)
                {
                    mapPix[mapRow + mapX] = s;
                }
                else
                {
                    // Simple Alpha Blending
                    float alpha = s.A / 255f;
                    Pixel target = mapPix[mapRow + mapX];

                    target.R = (byte)((s.R * alpha) + (target.R * (1 - alpha)));
                    target.G = (byte)((s.G * alpha) + (target.G * (1 - alpha)));
                    target.B = (byte)((s.B * alpha) + (target.B * (1 - alpha)));

                    mapPix[mapRow + mapX] = target;
                }
            }
        }
    }
}