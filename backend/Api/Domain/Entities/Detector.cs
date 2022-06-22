using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using System.Threading;

namespace Api.Domain.Entities
{
    public class Detector : IBaseEntity
    {
        public string Name { get; set; } = default!;
        public PhysicalAddress MacAddress { get; set; } = default!;
        public DetectorState State { get; set; }
    }

    // TODO(rg): Error state(s)
    public enum DetectorState
    {
        Off,
        Standby,
        Running
    }


}