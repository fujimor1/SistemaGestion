using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Compras;
using Termales.Common.DTOs.Compras;

namespace Termales.API.Controllers.Compras;

[ApiController]
[Route("api/compras")]
[Authorize(Roles = Modulos.Operaciones)]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _service;

    public ComprasController(ICompraService service) => _service = service;

    private string ObtenerUsuario() =>
        User.FindFirst(JwtRegisteredClaimNames.Name)?.Value
        ?? User.Identity?.Name
        ?? "Desconocido";

    [HttpGet]
    public async Task<IActionResult> ObtenerPaginado(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanoPagina = 10,
        [FromQuery] int? proveedorId = null,
        [FromQuery] string? estado = null)
    {
        var (items, total) = await _service.ObtenerPaginadoAsync(pagina, tamanoPagina, proveedorId, estado);
        return Ok(new { items, total });
    }

    [HttpGet("resumen-mes-actual")]
    public async Task<IActionResult> ObtenerResumenMesActual()
    {
        var resumen = await _service.ObtenerResumenMesActualAsync();
        return Ok(resumen);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var compra = await _service.ObtenerPorIdAsync(id);
        return compra is null ? NotFound() : Ok(compra);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarCompraDto dto)
    {
        try
        {
            var compra = await _service.RegistrarAsync(dto, ObtenerUsuario());
            return CreatedAtAction(nameof(ObtenerPorId), new { id = compra.CompraId }, compra);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id:int}/pagar")]
    public async Task<IActionResult> Pagar(int id, [FromBody] PagarCompraDto dto)
    {
        try
        {
            var compra = await _service.PagarAsync(id, dto, ObtenerUsuario());
            return Ok(compra);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>Sube una o más fotos del comprobante físico (boleta/factura en papel del
    /// proveedor). Máximo 8 MB por foto, solo JPG/PNG/WEBP.</summary>
    [HttpPost("{id:int}/imagenes")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> AgregarImagenes(int id, [FromForm] List<IFormFile> archivos)
    {
        try
        {
            var imagenes = await _service.AgregarImagenesAsync(id, archivos);
            return Ok(imagenes);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id:int}/imagenes")]
    public async Task<IActionResult> ObtenerImagenes(int id)
    {
        var imagenes = await _service.ObtenerImagenesAsync(id);
        return Ok(imagenes);
    }

    [HttpGet("imagenes/{imagenId:int}/archivo")]
    public async Task<IActionResult> ObtenerArchivoImagen(int imagenId)
    {
        var archivo = await _service.ObtenerArchivoImagenAsync(imagenId);
        if (archivo is null) return NotFound();
        return File(archivo.Value.Bytes, archivo.Value.ContentType, archivo.Value.NombreArchivo);
    }

    [HttpDelete("imagenes/{imagenId:int}")]
    public async Task<IActionResult> EliminarImagen(int imagenId)
    {
        try
        {
            await _service.EliminarImagenAsync(imagenId);
            return Ok(new { mensaje = "Imagen eliminada" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
