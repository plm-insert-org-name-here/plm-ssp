using System.Net.NetworkInformation;
using Domain.Common;
using Domain.Entities.CompanyHierarchy;

namespace Domain.Entities;

public class Detector : BaseEntity
{
    public string Name { get; set; } = default!;
    public PhysicalAddress MacAddress { get; set; } = default!;
    public DetectorState State { get; set; }

    public Location? Location { get; set; }
    public int? LocationId { get; set; }
}