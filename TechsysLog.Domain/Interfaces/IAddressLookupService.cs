using TechsysLog.Domain.Entities;

namespace TechsysLog.Domain.Interfaces;

public interface IAddressLookupService
{
    Task<ViaCepResponse?> GetAddressByZipCodeAsync(string zipCode);
}