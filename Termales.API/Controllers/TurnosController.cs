using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Modulos.BaniosHabitaciones)]
public class TurnosController : ControllerBase
{
    private readonly ITurnoService _service;

    public TurnosController(ITurnoService service) => _service = service;

    [HttpGet("tipos-servicio")]
    public async Task<IActionResult> ObtenerTiposServicio()
    {
        var resultado = await _service.ObtenerTiposServicioAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("por-tipo/{tipoServicioId:int}")]
    public async Task<IActionResult> ObtenerPorTipoYFecha(int tipoServicioId, [FromQuery] DateTime fecha)
    {
        var resultado = await _service.ObtenerPorTipoYFechaAsync(tipoServicioId, fecha);
        return Ok(resultado);
    }

    [HttpGet("disponibilidad/{tipoServicioId:int}")]
    public async Task<IActionResult> VerificarDisponibilidad(
        int tipoServicioId,
        [FromQuery] DateTime fecha,
        [FromQuery] int cantidadPersonas = 1)
    {
        var resultado = await _service.VerificarDisponibilidadAsync(tipoServicioId, fecha, cantidadPersonas);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("aforo")]
    public async Task<IActionResult> ObtenerAforoDelDia([FromQuery] DateTime? fecha)
    {
        var resultado = await _service.ObtenerAforoDelDiaAsync(fecha ?? DateTime.Today);
        return Ok(resultado);
    }

    [HttpPost("registrar-ingreso")]
    public async Task<IActionResult> RegistrarIngreso([FromBody] RegistrarTurnoDto dto)
    {
        var resultado = await _service.RegistrarIngresoAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.TurnoId }, resultado);
    }
}
