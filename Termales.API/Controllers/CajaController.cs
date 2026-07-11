using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs.Caja;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize(Roles = Modulos.Operaciones)]
public class CajaController : ControllerBase
{
    private readonly ICajaService _service;

    public CajaController(ICajaService service) => _service = service;

    private string ObtenerUsuario() =>
        User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
        ?? User.Identity?.Name
        ?? "Desconocido";

    // ── Apertura ──────────────────────────────────────────────────────────────

    [HttpGet("apertura/hoy")]
    public async Task<IActionResult> ObtenerAperturaHoy()
    {
        var apertura = await _service.ObtenerAperturaHoyAsync();
        return Ok(apertura);
    }

    [HttpPost("apertura")]
    public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaDto dto)
    {
        try
        {
            var apertura = await _service.AbrirCajaAsync(dto, ObtenerUsuario());
            return Ok(apertura);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // ── Egresos ───────────────────────────────────────────────────────────────

    [HttpGet("egresos/hoy")]
    public async Task<IActionResult> ObtenerEgresosHoy()
    {
        var egresos = await _service.ObtenerEgresosHoyAsync();
        return Ok(egresos);
    }

    [HttpGet("egresos")]
    public async Task<IActionResult> ObtenerEgresosPorFecha([FromQuery] DateTime fecha)
    {
        var egresos = await _service.ObtenerEgresosPorFechaAsync(fecha);
        return Ok(egresos);
    }

    [HttpPost("egresos")]
    public async Task<IActionResult> RegistrarEgreso([FromBody] RegistrarEgresoDto dto)
    {
        var egreso = await _service.RegistrarEgresoAsync(dto, ObtenerUsuario());
        return Ok(egreso);
    }

    [HttpDelete("egresos/{id:int}")]
    public async Task<IActionResult> EliminarEgreso(int id)
    {
        var ok = await _service.EliminarEgresoAsync(id);
        return ok ? NoContent() : NotFound();
    }

    // ── Cierre ────────────────────────────────────────────────────────────────

    [HttpGet("cierre/datos")]
    public async Task<IActionResult> ObtenerDatosCierre()
    {
        var datos = await _service.ObtenerDatosCierreAsync();
        return Ok(datos);
    }

    [HttpPost("cierre")]
    public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaDto dto)
    {
        try
        {
            var cierre = await _service.CerrarCajaAsync(dto);
            return Ok(cierre);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
