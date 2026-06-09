using System.Text.Json;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Infrastructure.ExternalServices;

public class ViaCepClient : IAddressLookupService
{
    private readonly HttpClient _httpClient;

    public ViaCepClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ViaCepResponse?> GetAddressByZipCodeAsync(string zipCode)
    {
        var cleanZipCode = zipCode.Replace("-", "").Trim();
        var url = $"https://viacep.com.br/ws/{cleanZipCode}/json/";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ViaCepResponse>(content);

        if (result is null || result.Error)
            return null;
            
        return result;
    }
}