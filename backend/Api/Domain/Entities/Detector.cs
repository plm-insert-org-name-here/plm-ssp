using System.Net.NetworkInformation;
using Api.Domain.Common;

namespace Api.Domain.Entities
{
    public class Detector : BaseEntity
    {
        public string Name { get; set; } = default!;
        public PhysicalAddress MacAddress { get; set; } = default!;
        public DetectorState State { get; set; }

        public Location? Location { get; set; }
        public int? LocationId { get; set; }
    }
}