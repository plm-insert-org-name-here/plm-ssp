namespace Domain.Entities.CompanyHierarchy;

public class Station : ICompanyHierarchyNode<Line, Location>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Location> Children { get; set; } = default!;
    public Line Parent { get; set; } = default!;
    public int ParentId { get; set; }
}