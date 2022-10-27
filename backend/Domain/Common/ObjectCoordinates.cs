using Microsoft.EntityFrameworkCore;

namespace Domain.Common;

[Owned]
public class ObjectCoordinates
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

}