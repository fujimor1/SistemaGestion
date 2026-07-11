using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Compras;
using Termales.Common.DTOs.Compras;

namespace Termales.API.Controllers.Compras;

[ApiController]
[Route("api/proveedores")]
[Authorize(Roles = Modulos.Operaciones)]
public class ProveedoresController : ControllerBase
{
    private readonly IProveedorService _service;

    public ProveedoresController(IProveedorService service) => _service = service;

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

    [HttpGet("ruc/{ruc}")]
    public async Task<IActionResult> ObtenerPorRuc(string ruc)
    {
        var resultado = await _service.ObtenerPorRucAsync(ruc);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearProveedorDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito)
            return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.ProveedorId }, resultado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProveedorDto dto)
    {
        if (id != dto.ProveedorId)
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
