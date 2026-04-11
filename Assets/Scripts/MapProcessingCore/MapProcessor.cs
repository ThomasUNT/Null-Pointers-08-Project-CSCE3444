using System;
using System.IO;
using MapProcessing.Core.Utils;

namespace MapProcessing.Core
{
    public class MapProcessor
    {
        // Textures
        private ImageData _waterTex, _grassTex, _desertTex, _tundraTex, _mountainTex, _forestTex;

        // Permanent Stages (Created once, reused forever)
        private EdgeRoughener _roughener = new EdgeRoughener();
        private MasterClipper _masterClipper = new MasterClipper();
        private CoastalStylizer _stylizer = new CoastalStylizer();
        private SpriteLibrary _spriteLibrary = new SpriteLibrary();
        private ObjectScanner _objectScanner = new ObjectScanner();
        private ObjectStamper _stamper;

        private float[,] _distanceBuffer, _internalDistanceBuffer;
        private ImageData _bufferA, _bufferB, _finalBuffer; // Reusable buffers to avoid allocations
        private ImageData _lastRoughenedMask;
        private ImageData _maskBuffer;

        public void Initialize(string textureFolder, int mapWidth, int mapHeight)
        {
            _waterTex = ImageLoader.Load(Path.Combine(textureFolder, "water.png"));
            _grassTex = ImageLoader.Load(Path.Combine(textureFolder, "grass.png"));
            _desertTex = ImageLoader.Load(Path.Combine(textureFolder, "desert.png"));
            _tundraTex = ImageLoader.Load(Path.Combine(textureFolder, "tundra.png"));
            _mountainTex = ImageLoader.Load(Path.Combine(textureFolder, "mountain.png"));
            _forestTex = ImageLoader.Load(Path.Combine(textureFolder, "forest.png"));

            _spriteLibrary.Initialize(textureFolder);
            _stamper = new ObjectStamper(_spriteLibrary);

            // Assign textures to clippers once
            _masterClipper.WaterTexture = _waterTex;
            _masterClipper.GrassTexture = _grassTex;
            _masterClipper.DesertTexture = _desertTex;
            _masterClipper.TundraTexture = _tundraTex;
            _masterClipper.MountainTexture = _mountainTex;
            _masterClipper.ForestTexture = _forestTex;

            _bufferA = new ImageData(mapWidth, mapHeight);
            _bufferB = new ImageData(mapWidth, mapHeight);
            _finalBuffer = new ImageData(mapWidth, mapHeight);
            _maskBuffer = new ImageData(mapWidth, mapHeight);
            _distanceBuffer = new float[mapWidth, mapHeight];
            _internalDistanceBuffer = new float[mapWidth, mapHeight];

            _roughener.PrecomputeWarp(mapWidth, mapHeight);
        }

        // THE SPRINT: Unity calls this every frame while drawing.
        // We only process the biome currently being drawn
        public ImageData ProcessLive(ImageData inputMask)
        {
            // Roughen the mask
            _roughener.Process(inputMask, _maskBuffer);
            _lastRoughenedMask = _maskBuffer;

            _masterClipper.ReferenceMask = _lastRoughenedMask;

            _masterClipper.Process(_bufferB, _bufferA);
            return _bufferA;
        }

        // THE MARATHON: Unity calls this on MouseUp.
        public ImageData ProcessFinal(ImageData sprintResult)
        {
            if (_lastRoughenedMask == null) return sprintResult;

            // Distance Map (Generates into pre-allocated [,] array)
            DistanceFieldGenerator.Generate(_lastRoughenedMask, _distanceBuffer);
            DistanceFieldGenerator.GenerateInternal(_lastRoughenedMask, _internalDistanceBuffer);


            // Styling (Reads from sprintResult, writes into _finalBuffer)
            _stylizer.DistanceMap = _distanceBuffer;
            _stylizer.ReferenceMask = _lastRoughenedMask;
            _stylizer.Process(sprintResult, _finalBuffer);

            _objectScanner.Scan(_lastRoughenedMask, _internalDistanceBuffer);

            _stamper.StampTops(_finalBuffer, _objectScanner.TopTrees);

            _masterClipper.ReferenceMask = _lastRoughenedMask;
            _masterClipper.Process(_finalBuffer, _finalBuffer, true); // Forest Only Pass

            _stamper.StampDepthObjects(_finalBuffer, _objectScanner.DepthObjects);

            return _finalBuffer;



            return _finalBuffer;
        }
    }
}