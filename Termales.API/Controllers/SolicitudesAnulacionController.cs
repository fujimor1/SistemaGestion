using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Comprobante;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/solicitudes-anulacion")]
[Authorize(Roles = Modulos.Operaciones)]
public class SolicitudesAnulacionController : ControllerBase
{
    private readonly ISolicitudAnulacionService _service;

    public SolicitudesAnulacionController(ISolicitudAnulacionService service)
    {
        _service = service;
    }

    private string ObtenerSupervisor() =>
        User.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? "Supervisor";

    [HttpGet("pendientes")]
    public async Task<IActionResult> ObtenerPendientes()
    {
        var lista = await _service.ObtenerPendientesAsync();
        return Ok(lista);
    }

    [HttpGet("historial")]
    public async Task<IActionResult> ObtenerHistorial([FromQuery] string? desde, [FromQuery] string? hasta)
    {
        var lista = await _service.ObtenerHistorialAsync(desde, hasta);
        return Ok(lista);
    }

    [HttpPut("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id)
    {
        var resultado = await _service.AprobarAsync(id, ObtenerSupervisor());
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("{id:int}/rechazar")]
    public async Task<IActionResult> Rechazar(int id, [FromBody] RechazarAnulacionDto dto)
    {
        var resultado = await _service.RechazarAsync(id, ObtenerSupervisor(), dto.MotivoRechazo);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}
