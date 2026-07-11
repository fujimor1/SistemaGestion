using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/paquetes-banio")]
[Authorize(Roles = Modulos.BaniosHabitaciones)]
public class PaquetesBanioController : ControllerBase
{
    private readonly IPaqueteBanioService _service;

    public PaquetesBanioController(IPaqueteBanioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerActivos()
    {
        var resultado = await _service.ObtenerActivosAsync();
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearPaqueteBanioDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPaqueteBanioDto dto)
    {
        if (id != dto.PaqueteBanioId)
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
