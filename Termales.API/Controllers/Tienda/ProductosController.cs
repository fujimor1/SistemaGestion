using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Tienda;
using Termales.Common.DTOs.Tienda;

namespace Termales.API.Controllers.Tienda;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _service;

    public ProductosController(IProductoService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 20,
        [FromQuery] string? busqueda = null)
    {
        var resultado = await _service.ObtenerPaginadoAsync(pagina, tamanoPagina, busqueda);
        return Ok(resultado);
    }

    [HttpGet("todos")]
    public async Task<IActionResult> ObtenerTodos()
    {
        var resultado = await _service.ObtenerTodosAsync();
        return Ok(resultado);
    }

    /// <summary>Todos los productos, activos o no — para Inventario y el selector de Compras
    /// (a diferencia de "todos", que es solo lo vendible en la Tienda).</summary>
    [HttpGet("todos-gestion")]
    public async Task<IActionResult> ObtenerTodosParaGestion()
    {
        var resultado = await _service.ObtenerTodosParaGestionAsync();
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var resultado = await _service.ObtenerPorIdAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("barcode/{codigo}")]
    public async Task<IActionResult> ObtenerPorCodigoBarras(string codigo)
    {
        var resultado = await _service.ObtenerPorCodigoBarrasAsync(codigo);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        var resultado = await _service.CrearAsync(dto);
        if (!resultado.Exito) return BadRequest(resultado);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Data!.ProductoId }, resultado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoDto dto)
    {
        var resultado = await _service.ActualizarAsync(id, dto);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Modulos.Operaciones)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _service.EliminarAsync(id);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }
}
