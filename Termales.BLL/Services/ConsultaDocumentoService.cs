using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Consultas;
using Termales.Common.Settings;
using Termales.Common.Wrappers;

namespace Termales.BLL.Services;

public class ConsultaDocumentoService : IConsultaDocumentoService
{
    private readonly HttpClient _http;
    private readonly ConsultaDocumentoSettings _cfg;

    public ConsultaDocumentoService(IHttpClientFactory httpFactory, IOptions<ConsultaDocumentoSettings> cfg)
    {
        _http = httpFactory.CreateClient("Decolecta");
        _cfg = cfg.Value;
    }

    public async Task<ApiResponse<ConsultaDniResultDto>> ConsultarDniAsync(string dni)
    {
        if (string.IsNullOrWhiteSpace(_cfg.ApiToken))
            return ApiResponse<ConsultaDniResultDto>.Fallido("Consulta de DNI no configurada");
        if (dni.Length != 8 || !dni.All(char.IsDigit))
            return ApiResponse<ConsultaDniResultDto>.Fallido("DNI inválido");

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"{_cfg.UrlBase.TrimEnd('/')}/api/v1/dni/{dni}");
            req.Headers.Add("Authorization", $"Bearer {_cfg.ApiToken}");

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return ApiResponse<ConsultaDniResultDto>.Fallido("No se encontró el DNI");

            var data = await resp.Content.ReadFromJsonAsync<ApisPeruDniResponse>();
            if (data is null || string.IsNullOrWhiteSpace(data.Nombres))
                return ApiResponse<ConsultaDniResultDto>.Fallido("No se encontró el DNI");

            var nombreCompleto = $"{data.Nombres} {data.ApellidoPaterno} {data.ApellidoMaterno}".Trim();
            return ApiResponse<ConsultaDniResultDto>.Exitoso(new ConsultaDniResultDto { Nombre = nombreCompleto });
        }
        catch (Exception)
        {
            return ApiResponse<ConsultaDniResultDto>.Fallido("Error al consultar el DNI");
        }
    }

    public async Task<ApiResponse<ConsultaRucResultDto>> ConsultarRucAsync(string ruc)
    {
        if (string.IsNullOrWhiteSpace(_cfg.ApiToken))
            return ApiResponse<ConsultaRucResultDto>.Fallido("Consulta de RUC no configurada");
        if (ruc.Length != 11 || !ruc.All(char.IsDigit))
            return ApiResponse<ConsultaRucResultDto>.Fallido("RUC inválido");

        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"{_cfg.UrlBase.TrimEnd('/')}/api/v1/ruc/{ruc}");
            req.Headers.Add("Authorization", $"Bearer {_cfg.ApiToken}");

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                return ApiResponse<ConsultaRucResultDto>.Fallido("No se encontró el RUC");

            var data = await resp.Content.ReadFromJsonAsync<ApisPeruRucResponse>();
            if (data is null || string.IsNullOrWhiteSpace(data.RazonSocial))
                return ApiResponse<ConsultaRucResultDto>.Fallido("No se encontró el RUC");

            return ApiResponse<ConsultaRucResultDto>.Exitoso(new ConsultaRucResultDto { RazonSocial = data.RazonSocial });
        }
        catch (Exception)
        {
            return ApiResponse<ConsultaRucResultDto>.Fallido("Error al consultar el RUC");
        }
    }

    private class ApisPeruDniResponse
    {
        [JsonPropertyName("nombres")]
        public string? Nombres { get; set; }

        [JsonPropertyName("apellidoPaterno")]
        public string? ApellidoPaterno { get; set; }

        [JsonPropertyName("apellidoMaterno")]
        public string? ApellidoMaterno { get; set; }
    }

    private class ApisPeruRucResponse
    {
        [JsonPropertyName("razonSocial")]
        public string? RazonSocial { get; set; }
    }
}
