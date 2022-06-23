namespace Api.Domain.Entities
{
    public class Location : IBaseEntity
    {
        public string Name { get; set; } = default!;
        public Detector? Detector { get; set; }
    }
}