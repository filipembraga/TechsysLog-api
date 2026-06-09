using System.Text.Json.Serialization;

namespace TechsysLog.Domain.Entities;

// <summary>
/// Maps the JSON response from the ViaCEP public API.
/// https://viacep.com.br/ws/{cep}/json/
/// </summary>
public class ViaCepResponse
{
    [JsonPropertyName("cep")]
    public string ZipCode { get; set; } = string.Empty;

    [JsonPropertyName("logradouro")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Neighborhood { get; set; } = string.Empty;

    [JsonPropertyName("localidade")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("erro")]
    public bool Error { get; set; }
}