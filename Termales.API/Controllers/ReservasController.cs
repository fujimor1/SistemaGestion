using Microsoft.AspNetCore.Mvc;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;
using Termales.Common.Helpers;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _service;

    public ReservasController(IReservaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado([FromQuery] FiltroReserva filtro)
    {
        var resultado = await _service.ObtenerPaginadoAsync(filtro);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<IActionResult> ObtenerPorCliente(int clienteId)
    {
        var resultado = await _service.ObtenerPorClienteAsync(clienteId);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearReservaDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.ReservaId }, resultado);
    }

    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoReservaDto dto)
    {
        var resultado = await _service.ActualizarEstadoAsync(id, dto);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id, [FromQuery] string? motivo)
    {
        var resultado = await _service.CancelarAsync(id, motivo);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}
