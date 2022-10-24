using System.Collections.Generic;

namespace Domain.Entities.CompanyHierarchy;

public class Station : BaseEntity
{
    public string Name { get; set; } = default!;

    public List<Location> Locations { get; set; } = default!;
    
}