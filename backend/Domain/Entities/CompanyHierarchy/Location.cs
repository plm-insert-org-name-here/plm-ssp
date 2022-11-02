using Domain.Common;
using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class Location : ICHNodeWithParent<Station>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public Station Parent { get; set; } = default!;
    public int ParentId { get; set; }
    public Detector? Detector { get; set; }
    
    public byte[]? Snapshot { get; set; }
    
    public Job? Job { get; set; }

    private Location() {}

    public Location(string name)
    {
        Name = name;
    }

    public Result Rename(string newName, ICHNameUniquenessChecker<Station, Location> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(Parent, newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name!");
        }

        Name = newName;

        return Result.Ok();
    }

    public Result AttachDetector(Detector newDetector)
    {
        if (Detector is null)
        {
            Detector = newDetector;
            return Result.Ok();
        }
        else
        {
            if (Detector.State == DetectorState.Off)
            {
                Detector = newDetector;
                return Result.Ok();
            }
            return Result.Fail("Location already have a detector attached to it!");
            
        }
    }

    public Result DetachDetector()
    {
        if (Detector is null)
        {
            return Result.Fail("Location already does not have a detector attached to it!");
        }
        else
        {
            if (Detector.State == DetectorState.Streaming)
            {
                return Result.Fail("An other detector currently running on this location!");
            }

            Detector = null;
            return Result.Ok();
        }
    }
}