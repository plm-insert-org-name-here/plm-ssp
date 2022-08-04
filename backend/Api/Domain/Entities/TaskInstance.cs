using System.Collections.Generic;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class TaskInstance : BaseEntity
    {
        public Task Task { get; set; } = default!;
        public List<Event> Events { get; set; } = default!;
        public TaskInstanceFinalState? FinalState { get; set; }
    }
}