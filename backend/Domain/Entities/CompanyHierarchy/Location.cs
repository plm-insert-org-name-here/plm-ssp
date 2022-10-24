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
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }

}