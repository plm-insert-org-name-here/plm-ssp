namespace Domain.Entities.CompanyHierarchy;

public class Line : ICompanyHierarchyNode<OPU, Station>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<Station> Children { get; set; } = default!;
    public OPU Parent { get; set; } = default!;
    public int ParentId { get; set; }
}