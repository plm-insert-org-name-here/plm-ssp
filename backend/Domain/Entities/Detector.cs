using System.Net.NetworkInformation;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Entities;

public class Detector : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public PhysicalAddress MacAddress { get; set; } = default!;
    public DetectorState State { get; set; }

    public Location? Location { get; set; }
    public int? LocationId { get; set; }

    private Detector() { }

    public Detector(string newName, PhysicalAddress newMacAddress, int newLocationId)
    {
        Name = newName;
        MacAddress = newMacAddress;
        LocationId = newLocationId;
    }
}