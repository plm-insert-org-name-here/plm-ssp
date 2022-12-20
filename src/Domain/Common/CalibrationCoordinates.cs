using Microsoft.EntityFrameworkCore;

namespace Domain.Common;

public class CalibrationCoordinates
{
    public int Id { get; set; }
    public List<Koordinates> Qr { get; set; } = default!;
    public List<Koordinates>? Tray { get; set; } = default!;

    public CalibrationCoordinates()
    {
        
    }
    public CalibrationCoordinates(List<Koordinates> x, List<Koordinates> y)
    {
        Qr = x;
        Tray = y;
    }

    [Owned]
    public class Koordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}