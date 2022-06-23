using System.Net.NetworkInformation;

namespace Api.Domain.Entities
{
    public class Detector : IBaseEntity
    {
        public string Name { get; set; } = default!;
        public PhysicalAddress MacAddress { get; set; } = default!;
        public DetectorState State { get; set; }

        public Location? Location { get; set; }
        public int? LocationId { get; set; }
    }

    // TODO(rg): Error state(s)
    public enum DetectorState
    {
        Off,
        Standby,
        Running
    }


}