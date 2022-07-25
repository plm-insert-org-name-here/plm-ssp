using System;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class Event : BaseEntity
    {
        public DateTime Timestamp { get; set; }
        public Template? Template { get; set; }
        public TaskInstance TaskInstance { get; set; } = default!;
        public EventResult Result { get; set; }
        public string? FailureReason { get; set; }
    }
}