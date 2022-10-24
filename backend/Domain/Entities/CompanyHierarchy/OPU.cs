using Domain.Interfaces;
using FluentResults;

namespace Domain.Entities.CompanyHierarchy;

public class OPU : ICHNodeWithParent<Site>, ICHNodeWithChildren<Line>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Line> Children { get; set; } = default!;
    public Site Parent { get; set; } = default!;
    public int ParentId { get; set; }

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