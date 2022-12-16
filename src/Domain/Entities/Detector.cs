using System.Net;
using System.Net.NetworkInformation;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;
using FluentResults;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public class Detector : IBaseEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public PhysicalAddress MacAddress { get; private set; } = default!;
    public IPAddress IpAddress { get; private set; } = default!;
    public DetectorState State { get; private set; }
    public List<HeartBeatLog> HeartBeatLogs { get; private set; } = default!;

    public Location? Location { get; private set; }
    public int? LocationId { get; private set; }

    [Owned]
    public class HeartBeatLog
    {
        public int Id { get; set; }
        public int Temperature { get; set; }
        public int FreeStoragePercentage { get; set; }
        public int Uptime { get; set; }
    }

    private Detector() { }

    private Detector(int id, string name, string macAddressString, string ipAddressString, DetectorState state, int locationId)
    {
        Id = id;
        Name = name;
        MacAddress = PhysicalAddress.Parse(macAddressString);
        IpAddress = IPAddress.Parse(ipAddressString);
        State = state;
        HeartBeatLogs = new List<HeartBeatLog>();
        LocationId = locationId;
    }

    public static Result<Detector> Create(string newName, PhysicalAddress newMacAddress, IPAddress newAddress, Location? location)
    {
        var detector = new Detector
        {
            Name = newName,
            MacAddress = newMacAddress,
            IpAddress = newAddress,
            State = DetectorState.Standby,
            HeartBeatLogs = new List<HeartBeatLog>()
        };

        if (location is null)
        {
            return Result.Ok(detector);
        }

        return location.AttachDetector(detector).ToResult(detector);
    }

    public void SetState(DetectorState state)
    {
        State = state;
    }

    public void AddToState(DetectorState state)
    {
        State |= state;
    }

    public void RemoveFromState(DetectorState state)
    {
        State &= ~state;
    }
}