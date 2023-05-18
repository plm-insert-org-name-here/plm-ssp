using Domain.Interfaces;
using FluentResults;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Entities.CompanyHierarchy;

public class Station : ICHNode<Line, Location>
{
    public int Id { get; private set; } = 1; 
    public string Name { get; private set; } = default!;
    public List<Location> Children { get; private set; } = default!;

    public Line Parent { get; set; } = default!;
    public int ParentId { get; private set; } = 1;

    private Station() {}
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
        if (Children.IsNullOrEmpty())
        {
            Children = new List<Location>();
        }
        Children.Add(location);

        return Result.Ok(location);
    }
}