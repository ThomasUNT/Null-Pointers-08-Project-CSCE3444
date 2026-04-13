using MapProcessing.Core;

public class MasterClipper
{
    public ImageData WaterTexture { get; set; }
    public ImageData GrassTexture { get; set; }
    public ImageData DesertTexture { get; set; }
    public ImageData TundraTexture { get; set; }
    public ImageData MountainTexture { get; set; }
    public ImageData ForestTexture { get; set; }
    public ImageData ReferenceMask { get; set; }

    public void Process(ImageData input, ImageData output, bool forestOnly = false)
    {
        Pixel[] inPix = input.Pixels;
        Pixel[] outPix = output.Pixels;
        Pixel[] maskPix = ReferenceMask.Pixels;

        // Texture arrays
        Pixel[] wTex = WaterTexture.Pixels;
        Pixel[] gTex = GrassTexture.Pixels;
        Pixel[] dTex = DesertTexture.Pixels;
        Pixel[] tTex = TundraTexture.Pixels;
        Pixel[] mTex = MountainTexture.Pixels;
        Pixel[] fTex = ForestTexture.Pixels;

        int width = input.Width;
        int height = input.Height;

        int texW = GrassTexture.Width;
        int texH = GrassTexture.Height;

        for (int y = 0; y < height; y++)
        {
            int mapRowOffset = y * width;
            int texY = y % texH;
            int texRowOffset = texY * texW;

            for (int x = 0; x < width; x++)
            {
                int i = mapRowOffset + x;
                Pixel m = maskPix[i];
                int texX = x % texW;
                int texIdx = texRowOffset + texX;

                if (forestOnly)
                {
                    if (IsForest(m)) outPix[i] = fTex[texIdx];
                    else outPix[i] = inPix[i]; // Keep original if not forest
                    continue; // Skip biome checks if we're only processing forests
                }

                // Single pass decision tree
                if (IsWater(m)) outPix[i] = wTex[texIdx];
                else if (IsGrass(m)) outPix[i] = gTex[texIdx];
                else if (IsDesert(m)) outPix[i] = dTex[texIdx];
                else if (IsTundra(m)) outPix[i] = tTex[texIdx];
                else if (IsMountain(m)) outPix[i] = mTex[texIdx];
                else if (IsForest(m)) outPix[i] = fTex[texIdx];
                else
                {
                    // If no biome matches, keep the original pixel
                    outPix[i] = inPix[i];
                }
            }
        }
    }

    private bool IsWater(Pixel p) => p.R == 255 && p.G == 255 && p.B == 255;
    private bool IsGrass(Pixel p) => p.R == 0 && p.G == 0 && p.B == 0;
    private bool IsDesert(Pixel p) => p.R == 255 && p.G == 235 && p.B == 4;
    private bool IsTundra(Pixel p) => p.R == 128 && p.G == 128 && p.B == 128;
    private bool IsMountain(Pixel p) => p.R == 255 && p.G == 0 && p.B == 0;
    private bool IsForest(Pixel p) => p.R == 0 && p.G == 255 && p.B == 0;
}