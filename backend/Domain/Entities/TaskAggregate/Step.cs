using Domain.Common;

namespace Domain.Entities
{
    public class Step : BaseEntity
    {
        public int? OrderNum { get; set; }
        public TemplateState ExpectedInitialState { get; set; }
        public TemplateState ExpectedSubsequentState { get; set; }
        public List<Event> Events { get; set; } = default!;
        public Template Template { get; set; } = default!;
        public int TemplateId { get; set; }
    }
}