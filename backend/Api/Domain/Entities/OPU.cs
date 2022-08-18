using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class OPU : BaseEntity
    {
        public string Name { get; set; } = default!;

        public List<Line> Lines { get; set; } = default!;

        public Site Site { get; set; } = default!;
        public int SiteId { get; set; }
    }
}