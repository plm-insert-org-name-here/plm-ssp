using System.Net;
using System.Net.NetworkInformation;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public class Detector : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public PhysicalAddress MacAddress { get; set; } = default!;
    public IPAddress? IpAddress { get; set; }
    public DetectorState State { get; set; }

    public Location? Location { get; set; }
    public int? LocationId { get; set; }

    private Detector() { }
    public List<HeartBeatLog> HearthBeatLogs { get; set; } = default!;

    [Owned]
    public class HeartBeatLog
    {
        public int Id { get; set; }
        public string Temperature { get; set; } = default!;
        public int FreeStoragePercentage { get; set; }
        public int Uptime { get; set; }
    }

    public Detector(string newName, PhysicalAddress newMacAddress, int newLocationId)
    {
        Name = newName;
        MacAddress = newMacAddress;
        LocationId = newLocationId;
        HearthBeatLogs = new List<HeartBeatLog>();
    }
}