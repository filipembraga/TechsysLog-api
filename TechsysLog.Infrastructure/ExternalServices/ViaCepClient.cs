using System.Text.Json;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Infrastructure.ExternalServices;

public class ViaCepClient : IAddressLookupService
{
    private readonly HttpClient _httpClient;

    public ViaCepClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // <summary>
    /// HTTP client implementation for the ViaCEP public API.
    /// Maps the provider-specific response to the domain-neutral AddressLookupResult.
    /// </summary
    public async Task<Address?> GetAddressByZipCodeAsync(string zipCode)
    {
        var cleanZipCode = zipCode.Replace("-", "").Trim();
        var response = await _httpClient.GetAsync($"{cleanZipCode}/json/");

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ViaCepResponseDto>(content);

        if (result is null || result.Erro)
            return null;

        return new Address(
             zipCode: result.Cep,
             street: result.Logradouro,
             number: string.Empty, // ViaCEP does not provide street number information
             neighborhood: result.Bairro,
             city: result.Localidade,
             state: result.Uf
        );
    }
}