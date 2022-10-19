using System.Collections.Generic;

namespace Domain.Entities.CompanyHierarchy;

public class Line : BaseEntity
{
    public string Name { get; set; } = default!;

    public List<Station> Stations { get; set; } = default!;

    public OPU OPU { get; set; } = default!;
    public int OPUId { get; set; }
}