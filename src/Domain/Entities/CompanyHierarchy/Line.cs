using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class Line : ICHNode<OPU, Station>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Station> Children { get; set; } = default!;

    public OPU Parent { get; set; } = default!;
    public int ParentId { get; set; }

    private Line() {}

    public Line(string name)
    {
        Name = name;
    }

    public Result<Station> AddChildNode(string stationName,
        ICHNameUniquenessChecker<Line, Station> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(this, stationName, null).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        var station = new Station(stationName);
        Children.Add(station);

        return Result.Ok(station);
    }

    public Result Rename(string newName, ICHNameUniquenessChecker<OPU, Line> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(Parent, newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }
}