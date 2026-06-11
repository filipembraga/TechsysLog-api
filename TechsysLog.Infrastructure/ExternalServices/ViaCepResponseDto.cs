using System.Text.Json.Serialization;

namespace TechsysLog.Infrastructure.ExternalServices;

// <summary>
/// Maps the JSON response from the ViaCEP public API.
/// Property names match the API's Portuguese JSON fields exactly
/// https://viacep.com.br/ws/{cep}/json/
/// </summary>
internal class ViaCepResponseDto
{
    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("logradouro")]
    public string Logradouro { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; } = string.Empty;

    [JsonPropertyName("localidade")]
    public string Localidade { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("erro")]
    public bool Erro { get; set; }
}