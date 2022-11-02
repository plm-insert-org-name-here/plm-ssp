using Domain.Common;
using Domain.Interfaces;
using FluentResults;
using Task = Domain.Entities.TaskAggregate.Task;

namespace Domain.Entities.CompanyHierarchy;

public class Location : ICHNodeWithParent<Station>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public Station Parent { get; set; } = default!;
    public int ParentId { get; set; }
    public Detector? Detector { get; set; }
    public List<Task> Tasks { get; set; } = default!;
    
    public byte[]? Snapshot { get; set; }

    private Location() {}

    public Location(string name)
    {
        Name = name;
    }

    public Result Rename(string newName, ICHNameUniquenessChecker<Station, Location> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(Parent, newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }

    public Result AttachDetector(Detector newDetector)
    {
        if (Detector is null)
        {
            Detector = newDetector;
            return Result.Fail("Location already have a detector attached to it");
        }
        else
        {
            return Result.Ok();
        }
    }
}