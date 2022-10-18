using Domain.Common;

namespace Domain.Entities
{
    public class TaskInstance : BaseEntity
    {
        public TaskInstanceFinalState? FinalState { get; set; }
        public List<Event> Events { get; set; } = default!;
        public Task Task { get; set; } = default!;
        public int TaskId { get; set; }
    }
}