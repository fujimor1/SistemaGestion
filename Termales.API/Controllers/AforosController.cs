using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Modulos.BaniosHabitaciones)]
public class AforosController : ControllerBase
{
    private readonly IAforoService _service;

    public AforosController(IAforoService service) => _service = service;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("por-fecha")]
    public async Task<IActionResult> ObtenerPorFecha([FromQuery] DateTime fecha)
    {
        var resultado = await _service.ObtenerPorFechaAsync(fecha);
        return Ok(resultado);
    }

    [HttpGet("por-tipo/{tipoServicioId:int}")]
    public async Task<IActionResult> ObtenerPorTipoYFecha(int tipoServicioId, [FromQuery] DateTime fecha)
    {
        var resultado = await _service.ObtenerPorTipoYFechaAsync(tipoServicioId, fecha);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearAforoDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.AforoId }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarAforoDto dto)
    {
        if (id != dto.AforoId)
            return BadRequest("El ID de la ruta no coincide con el cuerpo");

        var resultado = await _service.ActualizarAsync(dto);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }
}
