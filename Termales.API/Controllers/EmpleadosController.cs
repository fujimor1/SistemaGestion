using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Modulos.Sistema)]
public class EmpleadosController : ControllerBase
{
    private readonly IEmpleadoService _service;

    public EmpleadosController(IEmpleadoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] string? busqueda = null)
    {
        var resultado = await _service.ObtenerPaginadoAsync(pagina, tamanoPagina, busqueda);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("dni/{dni}")]
    public async Task<IActionResult> ObtenerPorDni(string dni)
    {
        var resultado = await _service.ObtenerPorDniAsync(dni);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearEmpleadoDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.EmpleadoId }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEmpleadoDto dto)
    {
        if (id != dto.EmpleadoId)
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
