using System.Collections.Generic;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class Job : BaseEntity
    {
        public string Name { get; set; } = default!;
        public JobType Type { get; set; }
        public byte[] Snapshot { get; set; } = default!;

        public Location? Location { get; set; }
        public int? LocationId { get; set; }

        public List<Task> Tasks { get; set; } = default!;
    }
}