using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces;
using Termales.Common.DTOs;

namespace Termales.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Modulos.BaniosHabitaciones)]
public class HabitacionesController : ControllerBase
{
    private readonly IHabitacionService _service;

    public HabitacionesController(IHabitacionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var resultado = await _service.ObtenerTodasAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearHabitacionDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.HabitacionId }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarHabitacionDto dto)
    {
        if (id != dto.HabitacionId)
            return BadRequest("El ID de la ruta no coincide con el cuerpo");

        var resultado = await _service.ActualizarAsync(dto);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPatch("{id:int}/ocupacion")]
    public async Task<IActionResult> CambiarOcupacion(int id, [FromQuery] bool ocupado)
    {
        var resultado = await _service.CambiarOcupacionAsync(id, ocupado);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPatch("{id:int}/marcar-limpia")]
    public async Task<IActionResult> MarcarLimpia(int id)
    {
        var resultado = await _service.MarcarLimpiaAsync(id);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _service.EliminarAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPatch("reordenar")]
    public async Task<IActionResult> Reordenar([FromBody] ReordenarHabitacionesDto dto)
    {
        var resultado = await _service.ReordenarAsync(dto);
        return Ok(resultado);
    }

    [HttpGet("{id:int}/items")]
    public async Task<IActionResult> ObtenerItems(int id)
    {
        var resultado = await _service.ObtenerItemsAsync(id);
        return Ok(resultado);
    }

    [HttpPost("{id:int}/items")]
    public async Task<IActionResult> AgregarItem(int id, [FromBody] CrearHabitacionItemDto dto)
    {
        var resultado = await _service.AgregarItemAsync(id, dto);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("items/{itemId:int}")]
    public async Task<IActionResult> EliminarItem(int itemId)
    {
        var resultado = await _service.EliminarItemAsync(itemId);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }
}
