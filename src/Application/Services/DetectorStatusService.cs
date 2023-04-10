using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public interface IDetectorStatusService
{
    public void AddHeartBeat(Detector detector);
    public void CheckStatus();
}

public class DetectorStatusService : IDetectorStatusService
{
    public IRepository<Detector> DetectorRepo = default!;
    private List< DetectorStateHolder> Detectors = new List< DetectorStateHolder>();
    public void AddHeartBeat(Detector detector)
    {
        var det = Detectors.FirstOrDefault(d => d.Detector.Id == detector.Id);
        if (det is null)
        {
            var newDet = new DetectorStateHolder
            {
                Detector = detector,
                TimeStamp = DateTime.Now,
                Alive = true
            };
        
            Detectors.Add(newDet);
        }
        else
        {
            det.TimeStamp = DateTime.Now;
        }
    }


    public async void CheckStatus()
    {
        while (true)
        {
            Thread.Sleep(1000000);
            IEnumerable<Detector>
            unavailableDetectors = Detectors.FindAll(d => d.TimeStamp.AddSeconds(5) < DateTime.Now).Select(d => d.Detector);

            foreach (var det in unavailableDetectors)
            {
                Console.WriteLine(det.Id);
                var detectorFromDb = await DetectorRepo.GetByIdAsync(det.Id);
                detectorFromDb?.SetState(DetectorState.Off);
            }

            await DetectorRepo.SaveChangesAsync();
        }
    }
}

class DetectorStateHolder
{
    public Detector Detector { get;set; }
    public DateTime TimeStamp { get; set; }
    public bool Alive { get; set; }
}