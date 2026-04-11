namespace MapProcessing.Core
{
    public enum MapObjectType { Mountain, ForestTop, ForestBottom }

    public struct MapObject
    {
        public int X;
        public int Y;
        public float InternalDistance; // Distance from the edge of its own biome
        public int VariantID; // which sprite variant to use
        public bool IsMountain;
    }
}