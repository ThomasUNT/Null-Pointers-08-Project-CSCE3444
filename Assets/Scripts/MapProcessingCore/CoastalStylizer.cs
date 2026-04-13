using System;

namespace MapProcessing.Core
{
    public class CoastalStylizer
    {
        public float[,] DistanceMap { get; set; }
        public ImageData ReferenceMask { get; set; }

        // --- Land Settings ---
        public bool EnableCoastDarkening = true;
        public float ShorelineDarkness = 0.5f;
        public float ShorelineWidth = 5.0f;

        // --- Water Settings ---
        public bool EnableWaterDepth = true;
        public float MaxDepthDistance = 300.0f; // How many pixels until "Max Dark"

        public bool EnableWaveHighlights = true;
        public bool TaperWaves = true;
        public float WaveSpacing = 8.0f; // Distance between wave highlights
        public float WaveDistance = 30.0f; // How far from shore to show wave highlights
        public float WaveBrightness = 40.0f; // How much to brighten wave highlights
        public float WaveThickness = 3.0f; // How thick the wave highlights are

        // A deep, saturated navy/teal
        public Pixel DeepWaterColor = new Pixel { R = 10, G = 30, B = 80, A = 255 };
        public float MaxDeepBlend = 0.8f; // 0.8 means 80% deep color, 20% original tex


        public void Process(ImageData input, ImageData output)
        {
            if (DistanceMap == null || ReferenceMask == null) return;

            Pixel[] inPix = input.Pixels;
            Pixel[] outPix = output.Pixels;
            Pixel[] maskPix = ReferenceMask.Pixels;

            int width = input.Width;
            int height = input.Height;

            for (int y = 0; y < height; y++)
            {
                int rowOffset = y * width;
                for (int x = 0; x < width; x++)
                {
                    int i = rowOffset + x;
                    float dist = DistanceMap[x, y];
                    Pixel maskP = maskPix[i];
                    Pixel texP = inPix[i]; // Start with the input color

                    if (IsLand(maskP))
                    {
                        if (EnableCoastDarkening && dist < ShorelineWidth)
                        {
                            float distancePercent = dist / ShorelineWidth;

                            float darkeningEffect = (1.0f - distancePercent) * ShorelineDarkness;
                            float factor = 1.0f - darkeningEffect;

                            texP.R = (byte)(texP.R * factor);
                            texP.G = (byte)(texP.G * factor);
                            texP.B = (byte)(texP.B * factor);
                        }
                    }
                    else
                    {
                        if (EnableWaterDepth)
                        {
                            float depthT = Math.Clamp(dist / MaxDepthDistance, 0f, 1f);
                            float blendAmount = depthT * MaxDeepBlend;

                            texP.R = (byte)Lerp(texP.R, DeepWaterColor.R, blendAmount);
                            texP.G = (byte)Lerp(texP.G, DeepWaterColor.G, blendAmount);
                            texP.B = (byte)Lerp(texP.B, DeepWaterColor.B, blendAmount);
                        }

                        if (EnableWaveHighlights && dist < WaveDistance && (dist % WaveSpacing) < WaveThickness)
                        {
                            float currentWaveBrightness = WaveBrightness;
                            if (TaperWaves)
                            {
                                currentWaveBrightness *= (1.0f - (dist / WaveDistance));
                            }

                            texP.R = (byte)Math.Min(255, texP.R + currentWaveBrightness);
                            texP.G = (byte)Math.Min(255, texP.G + currentWaveBrightness);
                            texP.B = (byte)Math.Min(255, texP.B + currentWaveBrightness);
                        }
                        
                    }
                    outPix[i] = texP; // Write the final modified pixel to the bucket
                }
            }
        }

        private float Lerp(float start, float end, float t) => start + (end - start) * t;

        private bool IsLand(Pixel p) => !(p.R == 255 && p.G == 255 && p.B == 255);
    }
}