using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class Station : ICHNode<Line, Location>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Location> Children { get; set; } = default!;

    public Line Parent { get; set; } = default!;
    public int ParentId { get; set; }

    public Station(string name)
    {
        Name = name;
    }

    public Result<Location> AddChildNode<TParent>(string locationName, ICHNameUniquenessChecker<TParent, Location> nameUniquenessChecker)
        where TParent: class, ICHNodeWithChildren<Location>
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