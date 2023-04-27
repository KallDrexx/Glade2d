using System.Collections.Generic;

namespace Glade2d;

public readonly record struct RenderRegion(int X, int Y, int Width, int Height);

internal class RenderRegionComparer : IComparer<RenderRegion>
{
    public int Compare(RenderRegion x, RenderRegion y)
    {
        var xComparison = x.X.CompareTo(y.X);
        if (xComparison != 0) return xComparison;
        
        var yComparison = x.Y.CompareTo(y.Y);
        if (yComparison != 0) return yComparison;
        
        var widthComparison = x.Width.CompareTo(y.Width);
        
        return widthComparison != 0 
            ? widthComparison 
            : x.Height.CompareTo(y.Height);
    }
}