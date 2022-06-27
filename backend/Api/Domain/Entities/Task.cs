using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class Task : BaseEntity
    {
        public string Name { get; set; } = default!;
        public bool Ordered { get; set; }
        public TaskStatus Status { get; set; }

        public Job Job { get; set; } = default!;
        public int JobId { get; set; }

        public List<Template> Templates { get; set; } = default!;

    }

    public enum TaskStatus
    {
        Active,
        Paused,
        Inactive
    }
}