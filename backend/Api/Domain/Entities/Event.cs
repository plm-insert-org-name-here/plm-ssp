using System;

namespace Api.Domain.Entities
{
    public class Event : BaseEntity
    {
        public DateTime Timestamp { get; set; }
        public Template? Template { get; set; }
        public TaskInstance TaskInstance { get; set; } = default!;
        public EventState State { get; set; }
        public string? FailureReason { get; set; }
    }

    public enum EventState
    {
        Success,
        Failure,
    }
}