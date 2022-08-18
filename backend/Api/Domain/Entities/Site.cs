using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class Site : BaseEntity
    {
        public string Name { get; set; } = default!;

        public List<OPU> OPUs { get; set; } = default!;
    }
}