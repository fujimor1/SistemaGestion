using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;

namespace Termales.API.Controllers.Inventario;

[ApiController]
[Route("api/inventario/productos")]
[Authorize(Roles = Modulos.Operaciones)]
public class EntradasProductoController : ControllerBase
{
    private readonly IEntradaProductoService _service;

    public EntradasProductoController(IEntradaProductoService service)
        => _service = service;

    [HttpGet("{productoId:int}/entradas")]
    public async Task<IActionResult> ObtenerEntradas(int productoId)
    {
        var entradas = await _service.ObtenerPorProductoAsync(productoId);
        return Ok(entradas);
    }

    [HttpPost("{productoId:int}/entradas")]
    public async Task<IActionResult> RegistrarEntrada(int productoId, [FromBody] RegistrarEntradaProductoDto dto)
    {
        dto.ProductoId = productoId;
        var entrada = await _service.RegistrarAsync(dto);
        return Ok(entrada);
    }
}
