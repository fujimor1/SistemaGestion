using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TipoServiciosController : ControllerBase
{
    private readonly ITipoServicioService _service;

    public TipoServiciosController(ITipoServicioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var resultado = await _service.ObtenerTodosAsync();
        return Ok(resultado);
    }

    [HttpGet("activos")]
    public async Task<IActionResult> ObtenerActivos()
    {
        var resultado = await _service.ObtenerActivosAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearTipoServicioDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.TipoServicioId }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTipoServicioDto dto)
    {
        if (id != dto.TipoServicioId)
            return BadRequest("El ID de la ruta no coincide con el cuerpo");

        var resultado = await _service.ActualizarAsync(dto);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var resultado = await _service.DesactivarAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }
}
