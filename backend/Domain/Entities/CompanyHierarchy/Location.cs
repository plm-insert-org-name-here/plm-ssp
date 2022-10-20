namespace Domain.Entities.CompanyHierarchy;

public class Location : ICompanyHierarchyNodeWithParent<Station>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public Station Parent { get; set; } = default!;
    public int ParentId { get; set; }
    public Detector? Detector { get; set; }
    public Job? Job { get; set; }

}