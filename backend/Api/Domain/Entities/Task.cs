using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class Task : BaseEntity
    {
        public string Name { get; set; } = default!;
        public TaskStatus Status { get; set; }
        public bool? Ordered { get; set; }
        public List<Template>? Templates { get; set; }

        public Job Job { get; set; } = default!;
        public int JobId { get; set; }
    }

    public enum TaskStatus
    {
        Active,
        Paused,
        Inactive
    }
}