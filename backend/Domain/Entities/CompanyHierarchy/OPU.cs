using System.Collections.Generic;

namespace Domain.Entities.CompanyHierarchy;

public class OPU : BaseEntity
{
    public string Name { get; set; } = default!;

    public List<Line> Lines { get; set; } = default!;

    public Site Site { get; set; } = default!;
    public int SiteId { get; set; }
}