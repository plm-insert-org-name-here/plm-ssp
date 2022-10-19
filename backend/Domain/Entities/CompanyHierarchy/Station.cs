namespace Domain.Entities.CompanyHierarchy;

public class Station : BaseEntity
{
    public string Name { get; set; } = default;

    public List<Location> Locations { get; set; } = default!;

    public Line Line { get; set; } = default!;
    public int LineId { get; set; }
}