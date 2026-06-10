using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Domain.Interfaces;

public interface IAddressLookupService
{
    Task<Address?> GetAddressByZipCodeAsync(string zipCode);
}