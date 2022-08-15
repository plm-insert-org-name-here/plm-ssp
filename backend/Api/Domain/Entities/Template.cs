using System.Collections.Generic;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class Template : BaseEntity
    {
        public string Name { get; set; } = default!;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Task Task { get; set; } = default!;
        public int TaskId { get; set; }

        public List<Event> Events { get; set; } = default!;
        public List<StateChange> StateChanges { get; set; } = default!;
    }
}