namespace Api.Domain.Entities
{
    public class Detector : IBaseEntity
    {
        public string Name { get; set; } = default!;
        public string MacAddress { get; set; } = default!;

        public DetectorState State { get; set; }
    }

    public enum DetectorState
    {
        Off,
        Standby,
        Running
    }
}