using System.Collections.Generic;

namespace Domain.Entities.CompanyHierarchy;

public class Site : BaseEntity
{
    public string Name { get; set; } = default!;

    public List<OPU> OPUs { get; set; } = default!;
    
}