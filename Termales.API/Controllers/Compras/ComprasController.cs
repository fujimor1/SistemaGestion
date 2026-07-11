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
}
