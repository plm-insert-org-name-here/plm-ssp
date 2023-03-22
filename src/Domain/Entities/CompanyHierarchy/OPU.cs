using Domain.Interfaces;
using FluentResults;
using Microsoft.IdentityModel.Tokens;

namespace Domain.Entities.CompanyHierarchy;

public class OPU : ICHNodeWithParent<Site>, ICHNodeWithChildren<Line>
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public List<Line> Children { get; private set; } = default!;
    public Site Parent { get; private set; } = default!;
    public int ParentId { get; private set; }

    private OPU() {}
    private OPU(int id, string name, int parentId)
    {
        Id = id;
        Name = name;
        ParentId = parentId;
    }

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
        if (Children.IsNullOrEmpty())
        {
            Children = new List<Line>();
        }
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