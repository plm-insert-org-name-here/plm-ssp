namespace Api.Domain.Entities
{
    public class Location : BaseEntity
    {
        public string Name { get; set; } = default!;
        public Detector? Detector { get; set; }
        public Job? Job { get; set; }
    }
}