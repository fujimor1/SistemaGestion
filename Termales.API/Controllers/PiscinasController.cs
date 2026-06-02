using Microsoft.AspNetCore.Mvc;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PiscinasController : ControllerBase
{
    private readonly IPiscinaService _service;

    public PiscinasController(IPiscinaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var resultado = await _service.ObtenerTodasAsync();
        return Ok(resultado);
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> ObtenerDisponibles()
    {
        var resultado = await _service.ObtenerDisponiblesAsync();
        return Ok(resultado);
    }

    [HttpGet("disponibles/fecha")]
    public async Task<IActionResult> ObtenerDisponiblesEnFecha(
        [FromQuery] DateTime ingreso,
        [FromQuery] DateTime salida)
    {
        var resultado = await _service.ObtenerDisponiblesEnFechaAsync(ingreso, salida);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPiscinaDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.PiscinaId }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPiscinaDto dto)
    {
        if (id != dto.PiscinaId)
            return BadRequest("El ID de la ruta no coincide con el cuerpo");

        var resultado = await _service.ActualizarAsync(dto);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPatch("{id:int}/disponibilidad")]
    public async Task<IActionResult> CambiarDisponibilidad(int id, [FromQuery] bool disponible)
    {
        var resultado = await _service.CambiarDisponibilidadAsync(id, disponible);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }
}
