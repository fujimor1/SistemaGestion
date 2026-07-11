using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Comedor;
using Termales.Common.DTOs.Comedor;
using Termales.Entities.Enums;

namespace Termales.API.Controllers.Comedor;

[ApiController]
[Route("api/comedor/ordenes")]
[Authorize(Roles = Modulos.ComedorOperacion)]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _service;

    public OrdenesController(IOrdenService service) => _service = service;

    [HttpGet("{id:int}")]
    [Authorize(Roles = Modulos.ComedorLectura)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("mesa/{mesaId:int}/activa")]
    [Authorize(Roles = Modulos.ComedorLectura)]
    public async Task<IActionResult> ObtenerActivaPorMesa(int mesaId)
    {
        var resultado = await _service.ObtenerActivaPorMesaAsync(mesaId);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("estado/{estado}")]
    [Authorize(Roles = Modulos.ComedorLectura)]
    public async Task<IActionResult> ObtenerPorEstado(EstadoOrden estado)
    {
        var resultado = await _service.ObtenerPorEstadoAsync(estado);
        return Ok(resultado);
    }

    [HttpGet("por-fecha")]
    [Authorize(Roles = Modulos.ComedorLectura)]
    public async Task<IActionResult> ObtenerPorFecha([FromQuery] DateTime? fecha)
    {
        var resultado = await _service.ObtenerPorFechaAsync(fecha ?? DateTime.Today);
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearOrdenDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito) return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.OrdenId }, resultado);
    }

    [HttpPost("{id:int}/items")]
    public async Task<IActionResult> AgregarItems(int id, [FromBody] AgregarItemsOrdenDto dto)
    {
        var resultado = await _service.AgregarItemsAsync(id, dto);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPatch("detalle/{detalleId:int}/estado")]
    public async Task<IActionResult> ActualizarEstadoDetalle(int detalleId, [FromBody] ActualizarEstadoDetalleDto dto)
    {
        var resultado = await _service.ActualizarEstadoDetalleAsync(detalleId, dto);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost("{id:int}/marcar-lista")]
    public async Task<IActionResult> MarcarLista(int id)
    {
        var resultado = await _service.MarcarListaAsync(id);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{id:int}/pasar-a-caja")]
    public async Task<IActionResult> PasarACaja(int id)
    {
        var resultado = await _service.PasarACajaAsync(id);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{id:int}/cerrar")]
    [Authorize(Roles = Modulos.ComedorLectura)]
    public async Task<IActionResult> CerrarOrden(int id)
    {
        var resultado = await _service.CerrarOrdenAsync(id);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var resultado = await _service.CancelarAsync(id);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }
}
