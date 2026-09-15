using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termales.API.Authorization;
using Termales.BLL.Interfaces.Inventario;
using Termales.Common.DTOs.Inventario;

namespace Termales.API.Controllers.Inventario;

[ApiController]
[Route("api/inventario/productos")]
[Authorize(Roles = Modulos.Operaciones)]
public class SalidasProductoController : ControllerBase
{
    private readonly ISalidaProductoService _service;

    public SalidasProductoController(ISalidaProductoService service)
        => _service = service;

    [HttpGet("{productoId:int}/salidas")]
    public async Task<IActionResult> ObtenerSalidas(int productoId)
    {
        var salidas = await _service.ObtenerPorProductoAsync(productoId);
        return Ok(salidas);
    }

    [HttpPost("{productoId:int}/salidas")]
    public async Task<IActionResult> RegistrarSalida(int productoId, [FromBody] RegistrarSalidaProductoDto dto)
    {
        dto.ProductoId = productoId;
        try
        {
            var salida = await _service.RegistrarAsync(dto);
            return Ok(salida);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
