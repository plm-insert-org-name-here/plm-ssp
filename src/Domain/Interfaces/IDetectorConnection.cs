using Domain.Common;
using Domain.Common.DetectorCommand;
using Domain.Entities;
using FluentResults;

namespace Domain.Interfaces;

// NOTE(rg): all methods take a Detector argument because the implementors might need properties of the Detector
// (e.g. HTTP implementation needs Detector.IPAddress)
public interface IDetectorConnection
{
    public Task<Result> SendCommand(Detector detector, DetectorCommand command);
    
    // TODO(rg): wrap byte[] into a more descriptive object (e.g. Image / Frame / Snapshot)
    public Task<Result<byte[]>> RequestSnapshot(Detector detector);
    
    public Task<Result<Stream>> RequestStream(Detector detector);

    public Task<Result<CalibrationCoordinates>> SendCalibrationData(Detector detector, CalibrationCoordinates coordinates,
        int[]? newTrayPoints);

    public Task<Result<byte[]>> RequestCalibrationPreview(Detector detector, CalibrationCoordinates coordinates,
        int[]? newTrayPoints);

}