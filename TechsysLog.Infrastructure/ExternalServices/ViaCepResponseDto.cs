using System.Text.Json.Serialization;

namespace TechsysLog.Infrastructure.ExternalServices;

// <summary>
/// Maps the JSON response from the ViaCEP public API.
/// Property names match the API's Portuguese JSON fields exactly
/// https://viacep.com.br/ws/{cep}/json/
/// </summary>
internal class ViaCepResponseDto
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Localidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public bool Erro { get; set; }
}