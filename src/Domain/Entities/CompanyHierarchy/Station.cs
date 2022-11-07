using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class Station : ICHNode<Line, Location>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Location> Children { get; set; } = default!;

    public Line Parent { get; set; } = default!;
    public int ParentId { get; set; }

    private Station() {}

    private Station(int id, string name, int parentId)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
    }

    public Station(string name)
    {
        Name = name;
    }

    public Result<Location> AddChildNode(string locationName, ICHNameUniquenessChecker<Station, Location> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(this, locationName, null).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        var location = new Location(locationName);
        Children.Add(location);

        return Result.Ok(location);
    }

    public Result Rename(string newName,
        ICHNameUniquenessChecker<Line, Station> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(Parent, newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }
}