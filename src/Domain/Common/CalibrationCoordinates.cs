using Microsoft.EntityFrameworkCore;

namespace Domain.Commonn;

public class CalibrationCoordinates
{
    public int Id { get; set; }
    public int[] Qr { get; set; }
    public int[] Tray { get; set; }

    public CalibrationCoordinates()
    {
        
    }
    public CalibrationCoordinates(int[] x, int[] y)
    {
        Qr = x;
        Tray = y;
    }
}