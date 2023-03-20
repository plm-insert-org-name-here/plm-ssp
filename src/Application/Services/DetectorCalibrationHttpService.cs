using Domain.Entities;
using Domain.Entities.TaskAggregate;
using FluentResults;

namespace Application.Services;

public class DetectorCalibrationHttpService
{
    private readonly HttpClient _client;
    public const string Scheme = "http";
    public const int Port = 3000;

    public DetectorCalibrationHttpService(HttpClient client)
    {
        _client = client;
    }

    public class CalibrationRequestResult
    {
        public byte[] Snapshot = default!;
        public List<Coordinate> Coordinates = default!;
    }

    public List<Coordinate> mapStringToCoordinates(string coordString)
    {
        // string[] coordList = coordString.ToList()
        List<Coordinate> coordList = new List<Coordinate>();
        var splitted = coordString.Replace("[", "").Replace("]", "").Split(",").Select(Int32.Parse).ToList();
   
        for (int i = 0; i < splitted.Count(); i+= 2)
        {
            coordList.Add(new Coordinate(){X = splitted[i],Y = splitted[i+1]});
        }
    
        return coordList;
    }

    public async Task<Result<CalibrationRequestResult>> RequestSnapshotAndCoordinates(Detector detector, string type)
    {
        var response = await _client.GetAsync($"{Scheme}://{detector.IpAddress}:{Port}/snapshot?type={type}");

        var coordinates = response.Headers.GetValues("Marker-Coordinates").FirstOrDefault();
        if (coordinates is null)
        {
            return Result.Fail("coordinates not found");
        }

        Console.WriteLine(coordinates);
        var res = new CalibrationRequestResult
        {
            Snapshot = await response.Content.ReadAsByteArrayAsync(),
            Coordinates = mapStringToCoordinates(coordinates)
        };

        return Result.Ok(res);
    }
}