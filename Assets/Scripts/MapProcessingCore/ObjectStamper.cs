using System.Collections.Generic;
using MapProcessing.Core;

public class ObjectStamper
{
    private SpriteLibrary _library;

    public ObjectStamper(SpriteLibrary library)
    {
        _library = library;
    }

    // Layer 1: The "Under-Forest" trees
    public void StampTops(ImageData map, List<MapObject> topTrees)
    {
        ImageData tree = _library.GetTree();
        foreach (var obj in topTrees)
        {
            SpriteStamper.Stamp(map, tree, obj.X - 6, obj.Y - 8);
        }
    }

    // Layer 2: The Y-Sorted Mountains and Interior/Bottom Trees
    public void StampDepthObjects(ImageData map, List<MapObject> depthObjects)
    {
        foreach (var obj in depthObjects)
        {
            if (obj.IsMountain)
            {
                ImageData mnt = _library.GetMountain(obj.VariantID, obj.InternalDistance);
                // Center-Bottom anchor
                SpriteStamper.Stamp(map, mnt, obj.X - (mnt.Width / 2), obj.Y);
            }
            else
            {
                ImageData tree = _library.GetTree();
                SpriteStamper.Stamp(map, tree, obj.X - 6, obj.Y - 10);
            }
        }
    }
}