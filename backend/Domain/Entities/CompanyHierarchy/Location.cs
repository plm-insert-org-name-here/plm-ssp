namespace Domain.Entities.CompanyHierarchy;

public class Location : BaseEntity
{
    public string Name { get; set; } = default!;
    public Detector? Detector { get; set; }
    
    public byte[]? Snapshot { get; set; }
}