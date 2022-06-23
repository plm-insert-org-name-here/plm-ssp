using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class TaskResult : BaseEntity
    {
        public Task Task { get; set; } = default!;
        public List<Event> Events { get; set; } = default!;
        public TaskFinalState FinalState { get; set; }
    }

    public enum TaskFinalState {
        Completed,
        Abandoned,
        Failed
    }
}