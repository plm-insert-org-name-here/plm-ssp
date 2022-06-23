using System;

namespace Api.Domain.Entities
{
    public class Event : BaseEntity
    {
        public DateTime Timestamp { get; set; }
        public Template Template { get; set; } = default!;
    }
}