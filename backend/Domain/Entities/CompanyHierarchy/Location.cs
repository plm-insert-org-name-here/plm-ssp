namespace Domain.Entities.CompanyHierarchy;

public class Location : BaseEntity
{
    public string Name { get; set; } = default!;
    public Detector? Detector { get; set; }
    public Job? Job { get; set; }

    public Station Station { get; set; } = default!;
    public int StationId { get; set; }
}