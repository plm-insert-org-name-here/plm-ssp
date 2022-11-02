using System.Net.NetworkInformation;
using Ardalis.Specification;
using Domain.Entities;

namespace Domain.Specifications;

public sealed class DetectorByMacAddressSpec: Specification<Detector>
{
    public DetectorByMacAddressSpec(PhysicalAddress macAddress)
    {
        Query.Where(d => d.MacAddress.Equals(macAddress));
    }
}