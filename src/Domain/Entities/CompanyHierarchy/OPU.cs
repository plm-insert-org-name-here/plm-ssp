using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class OPU : ICHNodeWithParent<Site>, ICHNodeWithChildren<Line>
{
    public int Id { get; private set; } = 1;
    public string Name { get; private set; } = default!;
    public List<Line> Children { get; private set; } = default!;
    public Site Parent { get; private set; } = default!;
    public int ParentId { get; private set; } = 1;

    private OPU() {}
    public OPU(string name)
    {
        Name = name;
    }

    public Result<Line> AddChildNode(string lineName, ICHNameUniquenessChecker<OPU, Line> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(this, lineName, null).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        var line = new Line(lineName);
        Children.Add(line);

        return Result.Ok(line);
    }

    public Result Rename(string newName, ICHNameUniquenessChecker<Site, OPU> nameUniquenessChecker)
    {
        if (nameUniquenessChecker.IsDuplicate(Parent, newName, this).GetAwaiter().GetResult())
        {
            return Result.Fail("Duplicate name");
        }

        Name = newName;

        return Result.Ok();
    }
}