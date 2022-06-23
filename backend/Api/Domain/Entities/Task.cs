using System.Collections.Generic;

namespace Api.Domain.Entities
{
    public class Task : BaseEntity
    {
        public string Name { get; set; } = default!;
        public List<Template>? Templates { get; set; }
    }
}