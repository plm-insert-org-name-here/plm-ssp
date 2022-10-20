namespace Domain.Entities.CompanyHierarchy;

public class OPU : ICompanyHierarchyNode<Site, Line>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Line> Children { get; set; } = default!;
    public Site Parent { get; set; } = default!;
    public int ParentId { get; set; }
}