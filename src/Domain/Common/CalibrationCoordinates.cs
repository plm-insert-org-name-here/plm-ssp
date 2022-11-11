namespace Domain.Common;

public class CalibrationCoordinates
{
    public int X { get; set; }
    public int Y { get; set; }

    public CalibrationCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public bool IsValid()
    {
        if (X is < 0 or > 640) return false;
        if (Y is < 0 or > 480) return false;

        return true;
    }
}