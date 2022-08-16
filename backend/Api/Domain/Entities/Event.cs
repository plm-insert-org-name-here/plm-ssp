using System;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class Event : BaseEntity
    {
        public DateTime Timestamp { get; set; }
        public EventResult Result { get; set; }
        public string? FailureReason { get; set; }
        public StateChange? StateChange { get; set; }
        public int StateChangeId { get;set; }
        public TaskInstance TaskInstance { get; set; } = default!;
        public int TaskInstanceId { get; set; }
    }
}