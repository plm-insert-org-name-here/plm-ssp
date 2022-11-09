using Microsoft.EntityFrameworkCore;

namespace Domain.Common;

[Owned]
public class ObjectCoordinates
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    // TODO(rg): read constants from config?
    public bool IsValid()
    {
        if (X is < 0 or > 640) return false;
        if (Y is < 0 or > 480) return false;
        if (X + Width > 640) return false;
        if (Y + Height > 480) return false;

        return true;
    }

}