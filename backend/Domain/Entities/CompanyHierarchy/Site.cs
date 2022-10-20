namespace Domain.Entities.CompanyHierarchy;

public class Site : ICompanyHierarchyNodeWithChildren<OPU>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<OPU> Children { get; set; } = default!;
}