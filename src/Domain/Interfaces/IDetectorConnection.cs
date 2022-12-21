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
    public Task<Result<byte[]>> RequestSnapshot(Detector detector, string type);
    
    public Task<Result<Stream>> RequestStream(Detector detector);

    public Task<Result<List<CalibrationCoordinates.Koordinates>>> SendCalibrationData(Detector detector, CalibrationCoordinates coordinates,
        List<CalibrationCoordinates.Koordinates>? newTrayPoints);

    public Task<Result<byte[]>> RequestCalibrationPreview(Detector detector, CalibrationCoordinates coordinates,
        List<CalibrationCoordinates.Koordinates>? newTrayPoints);
    public Task<Result<byte[]>> RequestCollectData(Detector detector);

}
