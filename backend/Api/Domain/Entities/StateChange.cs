using System.Collections.Generic;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class StateChange : BaseEntity
    {
        public int? OrderNum { get; set; }
        public TemplateState ExpectedInitialState { get; set; }
        public TemplateState ExpectedSubsequentState { get; set; }
        public List<Event> Events { get; set; } = default!;
        public Template Template { get; set; } = default!;
        public int TemplateId { get; set; }
    }
}